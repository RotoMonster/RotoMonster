using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RotoMonster.Services
{
    public class FeedMedia
    {
        public string ImageUrl { get; set; }
        public string MediaType { get; set; }
        public string VideoUrl { get; set; }
        public int? DurationMillis { get; set; }
        public short DisplayOrder { get; set; }
    }

    public class FeedPlayer
    {
        public int PlayerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Confidence { get; set; }
        public bool IsExactMatch { get; set; }
    }

    public class FeedKeyword
    {
        public string Keyword { get; set; }
        public string Category { get; set; }
        public decimal Weight { get; set; }
    }

    public class FeedTweet
    {
        public string TweetId { get; set; }
        public int SportId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DateAdded { get; set; }
        public string Text { get; set; }
        public string ScreenUsername { get; set; }
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBlueVerified { get; set; }
        public bool IsNews { get; set; }
        public bool IsTop { get; set; }
        public int? Followers { get; set; }
        public string AiText { get; set; }
        public string TweetUrl { get; set; }
        public List<FeedMedia> Media { get; set; } = new List<FeedMedia>();
        public List<FeedPlayer> Players { get; set; } = new List<FeedPlayer>();
        public List<FeedKeyword> Keywords { get; set; } = new List<FeedKeyword>();

        public long NumericId => long.TryParse(TweetId, out var id) ? id : 0;
    }

    public class TwitterFeedService
    {
        private class GetTweetsResponse
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public List<FeedTweet> Tweets { get; set; } = new List<FeedTweet>();
        }

        private class AnalyzeResponse
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string Summary { get; set; }
        }

        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger<TwitterFeedService> _log;
        private List<FeedTweet> _tweets = new List<FeedTweet>();
        private DateTime _lastFetch = DateTime.MinValue;

        public string BaseUrl { get; }
        public string ApiKey { get; }
        public int SportId { get; }
        public int RefreshSeconds { get; }
        public int MaxTweets { get; }
        public string LastError { get; private set; }
        public DateTime LastFetch => _lastFetch;

        public bool Enabled => SportId > 0 && !string.IsNullOrWhiteSpace(ApiKey);

        public TwitterFeedService(IConfiguration config, ILogger<TwitterFeedService> log)
        {
            _log = log;
            var section = config.GetSection("TwitterApi");
            BaseUrl = (section["BaseUrl"] ?? "https://twitter.rotomonster.com").TrimEnd('/');
            ApiKey = section["ApiKey"];
            SportId = int.TryParse(section["SportId"], out var sid) ? sid : DefaultSportId(config["Sport"]);
            RefreshSeconds = int.TryParse(section["RefreshSeconds"], out var rs) && rs >= 10 ? rs : 60;
            MaxTweets = int.TryParse(section["MaxTweets"], out var mt) && mt > 0 ? Math.Min(mt, 500) : 300;
        }

        private static int DefaultSportId(string sport)
        {
            switch ((sport ?? "").ToUpperInvariant())
            {
                case "NBA": return 1;
                case "MLB": return 2;
                case "NFL": return 3;
                case "NHL": return 4;
                default: return 0;
            }
        }

        public async Task<List<FeedTweet>> GetTweetsAsync(CancellationToken ct = default)
        {
            if (!Enabled) return new List<FeedTweet>();

            if ((DateTime.UtcNow - _lastFetch).TotalSeconds >= RefreshSeconds)
            {
                await _lock.WaitAsync(ct);
                try
                {
                    if ((DateTime.UtcNow - _lastFetch).TotalSeconds >= RefreshSeconds)
                        await RefreshAsync(ct);
                }
                finally
                {
                    _lock.Release();
                }
            }

            return _tweets;
        }

        private async Task RefreshAsync(CancellationToken ct)
        {
            try
            {
                var newest = _tweets.Where(t => t.DateAdded.HasValue).Select(t => t.DateAdded.Value).DefaultIfEmpty().Max();
                var request = new Dictionary<string, object>
                {
                    ["sportId"] = SportId,
                    ["orderByDateAdded"] = true,
                    ["maxResults"] = _tweets.Count == 0 ? MaxTweets : 200
                };
                if (_tweets.Count > 0 && newest != default)
                    request["addedOnOrAfter"] = newest;

                var response = await PostAsync<GetTweetsResponse>("/api/tweets/GetTweets", request, ct);
                if (response == null || !response.Success)
                {
                    LastError = response?.ErrorMessage ?? "No response from the Twitter API.";
                    _lastFetch = DateTime.UtcNow;
                    return;
                }

                var merged = _tweets.ToDictionary(t => t.TweetId);
                foreach (var t in response.Tweets.Where(t => !string.IsNullOrEmpty(t.TweetId)))
                {
                    if (merged.TryGetValue(t.TweetId, out var existing) && string.IsNullOrEmpty(t.AiText))
                        t.AiText = existing.AiText;
                    merged[t.TweetId] = t;
                }

                _tweets = merged.Values
                    .OrderByDescending(t => t.CreatedDate)
                    .Take(MaxTweets)
                    .ToList();
                LastError = null;
                _lastFetch = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                _lastFetch = DateTime.UtcNow;
                _log.LogError(ex, "Twitter feed refresh failed");
            }
        }

        public FeedTweet Find(string tweetId)
        {
            return _tweets.FirstOrDefault(t => t.TweetId == tweetId);
        }

        public async Task<string> SummarizeAsync(FeedTweet tweet, CancellationToken ct = default)
        {
            if (!Enabled || tweet == null) return null;

            var analyzed = await PostAsync<AnalyzeResponse>("/api/ai/analyze",
                new Dictionary<string, object> { ["text"] = tweet.Text ?? "", ["tweetId"] = tweet.TweetId }, ct);
            if (analyzed == null || !analyzed.Success || string.IsNullOrWhiteSpace(analyzed.Summary))
            {
                LastError = analyzed?.ErrorMessage ?? "AI summary failed.";
                return null;
            }

            tweet.AiText = analyzed.Summary.Trim();
            try
            {
                await PostAsync<AnalyzeResponse>("/api/tweets/SetAiText",
                    new Dictionary<string, object> { ["tweetId"] = tweet.TweetId, ["aiText"] = tweet.AiText }, ct);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "SetAiText failed for {TweetId}", tweet.TweetId);
            }

            return tweet.AiText;
        }

        private async Task<T> PostAsync<T>(string path, object body, CancellationToken ct) where T : class
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + path))
            {
                request.Headers.Add("X-API-Key", ApiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                using (var response = await Http.SendAsync(request, ct))
                {
                    var text = await response.Content.ReadAsStringAsync(ct);
                    if (!response.IsSuccessStatusCode)
                    {
                        LastError = $"Twitter API returned {(int)response.StatusCode}";
                        return null;
                    }
                    return JsonSerializer.Deserialize<T>(text, Json);
                }
            }
        }
    }
}
