using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using RotoMonster.Services;
using RotoMonsterUI;

namespace RotoMonster.Pages
{
    public class TwitterColumn
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public List<TweetCardInput> Cards { get; set; } = new List<TweetCardInput>();
    }

    public class TwitterViewState
    {
        public string ClearedAt { get; set; }
        public bool Paused { get; set; }
        public bool ShowAll { get; set; }
        public string NewestAdded { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public List<TwitterColumn> Columns { get; set; } = new List<TwitterColumn>();
        public List<NewsCardInput> Statuses { get; set; } = new List<NewsCardInput>();
        public int? EditStatusId { get; set; }
    }

    [Authorize(Roles = "Admin")]
    public class TwitterViewModel : PageModel
    {
        private class RmPlayer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string TeamCode { get; set; }
        }

        private readonly TwitterFeedService _feed;
        private readonly RMDBContext _db;
        private readonly IRMData _data;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger<TwitterViewModel> _log;

        public TwitterViewModel(TwitterFeedService feed, RMDBContext db, IRMData data, IMemoryCache cache, IConfiguration config, ILogger<TwitterViewModel> log)
        {
            _feed = feed;
            _db = db;
            _data = data;
            _cache = cache;
            _config = config;
            _log = log;
        }

        public TwitterViewState State { get; set; } = new TwitterViewState();
        public int RefreshSeconds => _feed.RefreshSeconds;
        public bool Enabled => _feed.Enabled;

        public async Task OnGetAsync(CancellationToken ct)
        {
            ViewData["PageHeading"] = "Admin";
            await BuildAsync(null, null, ct);
        }

        public async Task<IActionResult> OnGetColumnsAsync(string clearedAt, bool showAll, CancellationToken ct)
        {
            State.ClearedAt = clearedAt;
            State.ShowAll = showAll;
            await BuildAsync(null, null, ct);
            return Partial("_TwitterViewColumns", State);
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            ViewData["PageHeading"] = "Admin";
            State.ClearedAt = Request.Form["clearedAt"];
            State.Paused = Request.Form["paused"] == "true";
            State.ShowAll = Request.Form["showAll"] == "true";

            var form = Request.Form.Keys.ToDictionary(k => k, k => Request.Form[k].FirstOrDefault(v => !string.IsNullOrEmpty(v)) ?? "");

            var news = new NewsCardService().Process(form);
            if (news.DeleteNewsId.HasValue || news.SavePressed || news.CancelPressed || news.ToggleEditNewsId.HasValue || news.SetTagPressed)
            {
                if (news.DeleteNewsId.HasValue)
                {
                    State.Error = await DeleteStatusAsync(news.DeleteNewsId.Value, ct);
                    if (State.Error == null) State.Message = "Deleted the status.";
                }
                else if (news.SavePressed && news.SaveNewsId.HasValue)
                {
                    State.Error = await SaveStatusAsync(news.SaveNewsId.Value, news, ct);
                    if (State.Error == null) State.Message = "Saved the status.";
                    else State.EditStatusId = news.SaveNewsId;
                }
                else if (news.SetTagPressed)
                {
                    State.EditStatusId = news.SetTagNewsId;
                }
                else if (news.ToggleEditNewsId.HasValue && !news.CancelPressed)
                {
                    State.EditStatusId = news.ToggleEditNewsId;
                }
                await BuildAsync(null, null, ct);
                return Page();
            }
            var result = new TweetCardService().Process(form);
            if (!result.TweetId.HasValue)
            {
                await BuildAsync(null, null, ct);
                return Page();
            }

            await _feed.GetTweetsAsync(ct);
            var tweet = _feed.Find(result.TweetId.Value.ToString(CultureInfo.InvariantCulture));

            if (tweet != null && result.AutoFillPressed)
            {
                var summary = await _feed.SummarizeAsync(tweet, ct);
                if (summary == null)
                    State.Error = _feed.LastError;
                else if (result.SelectedPlayerId.HasValue && string.IsNullOrWhiteSpace(result.NewsDetails))
                    result.NewsDetails = summary;
            }

            if (tweet != null && result.PostPressed)
            {
                var error = await PostNewsAsync(tweet, result, ct);
                if (error == null)
                {
                    State.Message = "Posted news for " + PlayerName(result.SelectedPlayerId);
                    result = new TweetCardResult { TweetId = result.TweetId };
                }
                else
                {
                    State.Error = error;
                }
            }

            await BuildAsync(tweet, result, ct);
            return Page();
        }

        private async Task<string> PostNewsAsync(FeedTweet tweet, TweetCardResult result, CancellationToken ct)
        {
            if (!result.SelectedPlayerId.HasValue) return "Pick a player first.";
            if (string.IsNullOrWhiteSpace(result.StatusTypeText)) return "Pick a status first.";

            var playerId = result.SelectedPlayerId.Value;
            var type = _data.GetPlayerStatusTypeByName(result.StatusTypeText.Trim());
            if (type == null) return "Unknown status: " + result.StatusTypeText;

            int? tagId = null;
            if (!string.IsNullOrWhiteSpace(result.StatusTypeTag))
            {
                var tagTitle = result.StatusTypeTag.Trim();
                tagId = await _db.PlayerStatusTagTypes.AsNoTracking()
                    .Where(t => t.Title == tagTitle)
                    .Select(t => (int?)t.Id)
                    .FirstOrDefaultAsync(ct);
            }

            var now = DateTime.UtcNow;
            var active = await _db.PlayerStatuses.Where(s => s.PlayerId == playerId && s.IsActive).ToListAsync(ct);
            foreach (var s in active)
            {
                s.IsActive = false;
                s.DateDeactivated = now;
            }

            _db.PlayerStatuses.Add(new RotoMonster.Core.PlayerStatus
            {
                PlayerId = playerId,
                IsActive = true,
                PlayerStatusTypeId = type.Id,
                PlayerStatusTagTypeId = tagId,
                OwningUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                DateAdded = now,
                Subject = Trim(result.NewsTitle),
                Comment = Trim(result.NewsDetails),
                Source = "@" + tweet.ScreenUsername,
                SourceUrl = Trim(result.SourceURL) ?? TweetUrl(tweet)
            });
            await _db.SaveChangesAsync(ct);
            return null;
        }

        private const int StatusLimit = 40;

        private Dictionary<int, RotoMonster.Core.PlayerStatus> _activeStatuses = new Dictionary<int, RotoMonster.Core.PlayerStatus>();

        private string TeamColor(string code)
        {
            var sport = _data.Sport != null ? (_data.Sport.Title ?? "") : "";
            var c = (code ?? "").Trim();
            if (sport == "NBA") return TeamColorHelper.GetNbaTeamColorVar(c);
            if (sport == "NFL" || sport == "NHL") return "var(--team-" + sport.ToLower() + "-" + c.ToLower() + ", var(--brand-primary))";
            return TeamColorHelper.GetTeamColorVar(c);
        }

        private Dictionary<int, List<DisplayPosition>> PlayerPositions()
        {
            return _cache.GetOrCreate("twitterview-positions", e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var season = _data.GetDefaultSeason();
                if (season == null) return new Dictionary<int, List<DisplayPosition>>();
                return _data.GetPlayerSeasonPositions(_data.GetDefaultFantasyProvider(), season)
                    .Where(p => p.Position != null && !string.IsNullOrWhiteSpace(p.Position.Abbreviation))
                    .GroupBy(p => p.PlayerId)
                    .ToDictionary(g => g.Key, g => g.Select(p => new DisplayPosition
                    {
                        Id = p.Position.Id,
                        Abbreviation = p.Position.Abbreviation,
                        Color = "var(--pos-" + p.Position.Abbreviation.Trim().ToLower() + ")"
                    }).ToList());
            });
        }

        private Dictionary<int, double> Ownership()
        {
            return _cache.GetOrCreate("twitterview-ownership", e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return _data.GetAllDefaultOwnershipPlayers(DateTime.UtcNow)
                    .GroupBy(o => o.PlayerId)
                    .ToDictionary(g => g.Key, g => g.First().OwnershipPercent);
            });
        }

        private DisplayPlayerInput Display(int playerId, string name, string teamCode)
        {
            List<DisplayPosition> positions;
            double own;
            return new DisplayPlayerInput
            {
                PlayerId = playerId,
                PlayerName = name,
                TeamCode = teamCode,
                TeamColor = string.IsNullOrEmpty(teamCode) ? null : TeamColor(teamCode),
                Positions = PlayerPositions().TryGetValue(playerId, out positions) ? positions.ToList() : new List<DisplayPosition>(),
                OwnershipPercent = Ownership().TryGetValue(playerId, out own) ? (int?)Math.Round(own) : null
            };
        }

        private InjuryBadgeInput StatusBadge(int playerId)
        {
            RotoMonster.Core.PlayerStatus ps;
            if (!_activeStatuses.TryGetValue(playerId, out ps) || ps.PlayerStatusType == null) return null;
            var label = (ps.PlayerStatusType.TextFormat ?? "").Replace("{t}", "").Trim();
            if (label.Length == 0) return null;
            return new InjuryBadgeInput
            {
                StatusAbbreviation = label,
                StatusText = ps.PlayerStatusType.Title,
                StatusColor = ps.PlayerStatusType.BackgroundColor,
                StatusDetails = ps.Subject
            };
        }

        private async Task BuildStatusesAsync(CancellationToken ct)
        {
            var options = StatusOptions();
            var dark = Request.Cookies["monster-theme"] == "dark";
            var sport = string.Equals(_config["Sport"], "MLB", StringComparison.OrdinalIgnoreCase) ? NewsCardSport.MLB : NewsCardSport.NBA;
            var byId = Players().Values.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First());

            var rows = await _db.PlayerStatuses.AsNoTracking()
                .Include(s => s.PlayerStatusType)
                .Include(s => s.PlayerStatusTagType)
                .Include(s => s.Player)
                .Where(s => s.IsActive && s.DateDeleted == null)
                .OrderByDescending(s => s.DateAdded)
                .Take(StatusLimit)
                .ToListAsync(ct);

            var now = DateTime.UtcNow;
            foreach (var s in rows)
            {
                RmPlayer rm;
                byId.TryGetValue(s.PlayerId, out rm);
                var name = rm != null ? rm.Name : (s.Player != null ? ((s.Player.FirstName ?? "") + " " + (s.Player.LastName ?? "")).Trim() : "");
                var typeTitle = s.PlayerStatusType != null ? s.PlayerStatusType.Title : "";
                var tagTitle = s.PlayerStatusTagType != null ? s.PlayerStatusTagType.Title : null;

                var ncDetails = (s.Comment ?? "").Trim();
                if (typeTitle.Length > 0 && ncDetails.StartsWith(typeTitle + " - ", StringComparison.OrdinalIgnoreCase)) ncDetails = ncDetails.Substring(typeTitle.Length + 3).Trim();
                var ncTitle = (s.Subject ?? "").Trim();
                if (ncTitle.Length > 0 && ncDetails.IndexOf(ncTitle, StringComparison.OrdinalIgnoreCase) >= 0) ncTitle = "";
                State.Statuses.Add(new NewsCardInput
                {
                    NewsId = s.Id,
                    DisplayPlayerInput = Display(s.PlayerId, name, rm != null ? rm.TeamCode : ""),
                    Sport = sport,
                    IsDarkMode = dark,
                    StatusTypeText = typeTitle,
                    StatusTypeColorCode = s.PlayerStatusType != null ? s.PlayerStatusType.BackgroundColor : null,
                    CurrentStatusTypeColorCode = s.PlayerStatusType != null ? s.PlayerStatusType.BackgroundColor : null,
                    StatusTypeTag = tagTitle,
                    CurrentStatusTypeText = typeTitle,
                    CurrentStatusTypeTag = tagTitle,
                    CurrentStatusNewsTitle = s.Subject,
                    NewsTitle = ncTitle.Length > 0 ? ncTitle : null,
                    NewsDetails = ncDetails.Length > 0 ? ncDetails : null,
                    SourceURL = s.SourceUrl,
                    TimeSinceCreated = now - s.DateAdded,
                    UserCanEdit = true,
                    UserCanDelete = true,
                    IsEditing = State.EditStatusId == s.Id,
                    StatusTypeOptions = options.Item1,
                    StatusTypeTagOptions = options.Item2
                });
            }
        }

        private async Task<string> SaveStatusAsync(int id, NewsCardResult r, CancellationToken ct)
        {
            var s = await _db.PlayerStatuses.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (s == null) return "That status no longer exists.";

            if (!string.IsNullOrWhiteSpace(r.StatusTypeText))
            {
                var type = _data.GetPlayerStatusTypeByName(r.StatusTypeText.Trim());
                if (type == null) return "Unknown status: " + r.StatusTypeText;
                s.PlayerStatusTypeId = type.Id;
            }

            int? tagId = null;
            if (!string.IsNullOrWhiteSpace(r.StatusTypeTag))
            {
                var tagTitle = r.StatusTypeTag.Trim();
                tagId = await _db.PlayerStatusTagTypes.AsNoTracking()
                    .Where(t => t.Title == tagTitle)
                    .Select(t => (int?)t.Id)
                    .FirstOrDefaultAsync(ct);
            }

            s.PlayerStatusTagTypeId = tagId;
            s.Subject = Trim(r.NewsTitle);
            s.Comment = Trim(r.NewsDetails);
            s.SourceUrl = Trim(r.SourceURL);
            await _db.SaveChangesAsync(ct);
            return null;
        }

        private async Task<string> DeleteStatusAsync(int id, CancellationToken ct)
        {
            var s = await _db.PlayerStatuses.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (s == null) return "That status no longer exists.";

            var now = DateTime.UtcNow;
            s.IsActive = false;
            s.DateDeactivated = now;
            s.DateDeleted = now;
            s.DeletedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _db.SaveChangesAsync(ct);
            return null;
        }

        private static string Trim(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private string PlayerName(int? playerId)
        {
            if (!playerId.HasValue) return "";
            var p = Players().Values.FirstOrDefault(x => x.Id == playerId.Value);
            return p?.Name ?? "";
        }

        private async Task BuildAsync(FeedTweet active, TweetCardResult result, CancellationToken ct)
        {
            await BuildStatusesAsync(ct);

            _activeStatuses = (await _data.GetActivePlayerStatusesAsync())
                .GroupBy(s => s.PlayerId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.DateAdded).First());

            if (!_feed.Enabled)
            {
                State.Error = "Twitter isn't set up for this sport. Add TwitterApi:ApiKey (and TwitterApi:SportId for NFL/NHL) to appsettings.";
                return;
            }

            var tweets = await _feed.GetTweetsAsync(ct);
            if (State.Error == null && _feed.LastError != null) State.Error = _feed.LastError;

            var newest = tweets.Where(t => t.DateAdded.HasValue).Select(t => t.DateAdded.Value).DefaultIfEmpty().Max();
            State.NewestAdded = newest == default ? "" : newest.ToString("o", CultureInfo.InvariantCulture);

            DateTime clearedAt;
            var hasCleared = DateTime.TryParse(State.ClearedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out clearedAt);

            var visible = tweets
                .Where(t => t.NumericId > 0)
                .Where(t => !hasCleared || !t.DateAdded.HasValue || t.DateAdded.Value > clearedAt || (active != null && t.TweetId == active.TweetId))
                .ToList();

            var options = StatusOptions();
            var dark = Request.Cookies["monster-theme"] == "dark";
            var sport = string.Equals(_config["Sport"], "MLB", StringComparison.OrdinalIgnoreCase) ? NewsCardSport.MLB : NewsCardSport.NBA;

            var activeShown = false;
            Func<FeedTweet, TweetCardInput> card = t =>
            {
                var isActive = !activeShown && active != null && result != null && t.TweetId == active.TweetId;
                if (isActive) activeShown = true;
                return ToCard(t, isActive ? result : null, options, dark, sport);
            };

            State.Columns.Add(new TwitterColumn { Key = "news", Title = "News", Cards = visible.Where(t => t.IsNews).Select(card).ToList() });
            State.Columns.Add(new TwitterColumn { Key = "top", Title = "Top", Cards = visible.Where(t => t.IsTop && !t.IsNews).Select(card).ToList() });
            State.Columns.Add(new TwitterColumn { Key = "all", Title = "All", Cards = visible.Where(t => !t.IsNews && !t.IsTop).Select(card).ToList() });
        }

        private Tuple<List<string>, List<string>> StatusOptions()
        {
            return _cache.GetOrCreate("twitterview-status-options", e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var types = _db.PlayerStatusTypes.AsNoTracking().OrderBy(t => t.Title).Select(t => t.Title).ToList();
                var tags = _db.PlayerStatusTagTypes.AsNoTracking().OrderBy(t => t.DisplayOrder).Select(t => t.Title).ToList();
                return Tuple.Create(types, tags);
            });
        }

        private static string NameKey(string first, string last)
        {
            var s = ((first ?? "") + " " + (last ?? "")).ToLowerInvariant();
            var chars = s.Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => char.IsLetter(c) || c == ' ')
                .ToArray();
            return string.Join(" ", new string(chars).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w != "jr" && w != "sr" && w != "ii" && w != "iii" && w != "iv"));
        }

        private Dictionary<string, RmPlayer> Players()
        {
            return _cache.GetOrCreate("twitterview-players", e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var map = new Dictionary<string, RmPlayer>();
                var season = _data.GetDefaultSeason();
                if (season == null) return map;

                var rows = _db.SeasonPlayers.AsNoTracking()
                    .Where(sp => sp.SeasonId == season.Id)
                    .Select(sp => new { sp.PlayerId, sp.Player.FirstName, sp.Player.LastName, TeamCode = sp.Team.Code })
                    .ToList();

                foreach (var r in rows)
                {
                    var key = NameKey(r.FirstName, r.LastName);
                    if (!map.ContainsKey(key))
                        map[key] = new RmPlayer { Id = r.PlayerId, Name = (r.FirstName + " " + r.LastName).Trim(), TeamCode = r.TeamCode };
                }
                return map;
            });
        }

        private static string TweetUrl(FeedTweet t)
        {
            if (!string.IsNullOrEmpty(t.TweetUrl)) return t.TweetUrl;
            return string.IsNullOrEmpty(t.ScreenUsername) || t.NumericId == 0 ? "" : "https://x.com/" + t.ScreenUsername + "/status/" + t.TweetId;
        }

        private static TweetMediaType MediaType(string type)
        {
            switch ((type ?? "").ToLowerInvariant())
            {
                case "video": return TweetMediaType.Video;
                case "animated_gif":
                case "gif": return TweetMediaType.Gif;
                default: return TweetMediaType.Photo;
            }
        }

        private TweetCardInput ToCard(FeedTweet t, TweetCardResult result, Tuple<List<string>, List<string>> options, bool dark, NewsCardSport sport)
        {
            var players = Players();
            var cardPlayers = new List<TweetCardPlayer>();
            var seen = new HashSet<int>();
            foreach (var p in t.Players ?? new List<FeedPlayer>())
            {
                RmPlayer rm;
                if (!players.TryGetValue(NameKey(p.FirstName, p.LastName), out rm) || !seen.Add(rm.Id)) continue;
                var dpi = Display(rm.Id, rm.Name, rm.TeamCode);
                dpi.IsExactMatch = p.IsExactMatch;
                dpi.FirstName = p.FirstName;
                dpi.LastName = p.LastName;
                cardPlayers.Add(new TweetCardPlayer
                {
                    DisplayPlayerInput = dpi,
                    MatchConfidence = (double)p.Confidence,
                    PercentText = dpi.OwnershipPercent.HasValue ? dpi.OwnershipPercent.Value.ToString() : null,
                    InjuryBadge = StatusBadge(rm.Id)
                });
            }

            cardPlayers = cardPlayers
                .OrderByDescending(cp => cp.DisplayPlayerInput.IsExactMatch)
                .ThenBy(cp => cp.DisplayPlayerInput.TeamCode ?? "", StringComparer.OrdinalIgnoreCase)
                .ThenBy(cp => cp.DisplayPlayerInput.LastName ?? "", StringComparer.OrdinalIgnoreCase)
                .ThenBy(cp => cp.DisplayPlayerInput.FirstName ?? "", StringComparer.OrdinalIgnoreCase)
                .ToList();

            var teams = cardPlayers
                .Select(p => p.DisplayPlayerInput.TeamCode)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .Select(c => new TweetCardTeam { TeamCode = c, Name = c })
                .ToList();

            var input = new TweetCardInput
            {
                TweetId = t.NumericId,
                ScreenName = t.ScreenUsername,
                DisplayName = t.DisplayName,
                Followers = t.Followers,
                TweetUrl = TweetUrl(t),
                TimeSinceCreated = DateTime.UtcNow - t.CreatedDate,
                ProfileImageUrl = t.ImageUrl,
                IsVerified = t.IsVerified || t.IsBlueVerified,
                Text = t.Text,
                HideMedia = true,
                AiText = t.AiText,
                Media = (t.Media ?? new List<FeedMedia>()).OrderBy(m => m.DisplayOrder).Select(m => new TweetCardMedia
                {
                    MediaType = MediaType(m.MediaType),
                    ImageUrl = m.ImageUrl,
                    VideoUrl = m.VideoUrl,
                    DurationMillis = m.DurationMillis
                }).ToList(),
                Keywords = (t.Keywords ?? new List<FeedKeyword>()).Select(k => new TweetCardKeyword
                {
                    Text = k.Keyword,
                    Category = k.Category,
                    Weight = (double)k.Weight
                }).ToList(),
                Players = cardPlayers,
                Teams = teams,
                Sport = sport,
                IsDarkMode = dark,
                StatusTypeOptions = options.Item1,
                StatusTypeTagOptions = options.Item2
            };

            if (result != null)
            {
                input.IsScrollTarget = true;
                input.SelectedTeamCode = result.SelectedTeamCode;
                input.SelectedPlayerId = result.SelectedPlayerId;
                input.StatusTypeText = result.StatusTypeText;
                input.StatusTypeTag = result.StatusTypeTag;
                input.NewsTitle = result.NewsTitle;
                input.NewsDetails = result.NewsDetails;
                input.SourceURL = result.SourceURL;
                input.IsUnofficial = result.IsUnofficial;
            }

            return input;
        }
    }
}
