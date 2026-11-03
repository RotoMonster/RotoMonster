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
    }

    [Authorize(Roles = "Admin,Worker")]
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
            ViewData["PageHeading"] = "Twitter";
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
            ViewData["PageHeading"] = "Twitter";
            State.ClearedAt = Request.Form["clearedAt"];
            State.Paused = Request.Form["paused"] == "true";
            State.ShowAll = Request.Form["showAll"] == "true";

            var form = Request.Form.Keys.ToDictionary(k => k, k => Request.Form[k].FirstOrDefault(v => !string.IsNullOrEmpty(v)) ?? "");
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
            State.Columns.Add(new TwitterColumn { Key = "top", Title = "Top", Cards = visible.Where(t => t.IsTop).Select(card).ToList() });
            if (State.ShowAll)
                State.Columns.Add(new TwitterColumn { Key = "all", Title = "All", Cards = visible.Select(card).ToList() });
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
                cardPlayers.Add(new TweetCardPlayer
                {
                    DisplayPlayerInput = new DisplayPlayerInput { PlayerId = rm.Id, PlayerName = rm.Name, TeamCode = rm.TeamCode },
                    MatchConfidence = (double)p.Confidence
                });
            }

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
