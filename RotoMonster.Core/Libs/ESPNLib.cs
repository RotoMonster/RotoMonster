using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RotoMonster.Core.Libs
{
    public class ESPNLib
    {
        private readonly IConfiguration config;
        private readonly ILogger logger;
        private FileLib filelib = new FileLib();

        public ESPNLib(IConfiguration config, ILogger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        //public string GetESPNSport(string sport)
        //{
        //    if (sport == "NBA")
        //        return "FBA";
        //    if (sport == "NFL")
        //        return "FFL";
        //    if (sport == "MLB")
        //        return "FLB";

        //    return "";
        //}

        public string CleanSWID(string swid)
        {
            string outSWID = swid;
            outSWID = swid.Replace("{", "");
            outSWID = outSWID.Replace("}", "");
            outSWID = outSWID.Trim();

            return outSWID;
        }

        /// <summary>
        /// How far ahead rosters are read. See where it is used - it is about
        /// pending changes rather than about the future.
        /// </summary>
        private const int RosterDaysAhead = 15;

        public string ReadESPNUrl(Sport sport, string espnYear, string leagueId, UserAuth userAuth, string tags = "mScoreboard&view=mTeam&view=mLiveScoring&view=mMatchupScore")
        {
            if (userAuth.ESPNswid.Length == 0 | userAuth.ESPNs2.Length == 0)
                return "";

            // Sport and season come from the caller. These used to be
            // hardcoded to fba and 2026, so every non basketball import asked
            // ESPN for a basketball league and failed.
            string espnCode = (sport != null && !string.IsNullOrEmpty(sport.ESPNCode))
                ? sport.ESPNCode.ToLower()
                : "fba";

            string season = string.IsNullOrEmpty(espnYear) ? "2026" : espnYear;

            string url = $"https://lm-api-reads.fantasy.espn.com/apis/v3/games/{espnCode}/seasons/{season}/segments/0/leagues/{leagueId}?{tags}";

            string data = "";

            try
            {
                using (var web = new WebClient())
                {
                    web.Encoding = Encoding.UTF8;
                    web.Headers.Add("Cookie", $"SWID={{{userAuth.ESPNswid}}}; espn_s2={userAuth.ESPNs2};");

                    if (url.Contains("kona_player_info"))
                    {
                        web.Headers.Add("x-fantasy-filter", "{\"players\":{\"filterStatus\":{\"value\":[\"FREEAGENT\",\"WAIVERS\"]},\"filterSlotIds\":{\"value\":[0,1,2,3,4,5,6,7,8,9,10,11]},\"filterRanksForScoringPeriodIds\":{\"value\":[1]},\"limit\":1000,\"offset\":0,\"sortPercOwned\":{\"sortAsc\":false,\"sortPriority\":1},\"sortDraftRanks\":{\"sortPriority\":100,\"sortAsc\":true,\"value\":\"STANDARD\"},\"filterRanksForRankTypes\":{\"value\":[\"STANDARD\"]},\"filterStatsForTopScoringPeriodIds\":{\"value\":5,\"additionalValue\":[\"002025\",\"102025\",\"002024\",\"012025\",\"022025\",\"032025\",\"042025\"]}}}");
                    }

                    data = web.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                // The original message said nothing about what failed, which
                // made a wrong sport in the url look the same as a bad cookie.
                throw new Exception("Could not read league " + leagueId
                    + " from ESPN [" + espnCode + " " + season + "]: " + ex.Message);
            }

            return data;

            //string url = "https://fantasy.espn.com/apis/v3/games/" + sport.ESPNCode.ToLower() + "/seasons/" + espnYear + "/segments/0/leagues/" + leagueId + "?view=" + tags;

            //if (userAuth.ESPNswid.Length > 0 & userAuth.ESPNs2.Length > 0)
            //{
            //    try
            //    {
            //        string results = "";
            //        if (userAuth.ESPNswid.Length > 0 & userAuth.ESPNs2.Length > 0)
            //        {
            //            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            //            req.Host = "fantasy.espn.com";
            //            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:62.0) Gecko/20100101 Firefox/62.0";
            //            req.Accept = "application/json";
            //            req.Referer = "http://fantasy.espn.com/basketball/league/rosters?leagueId=34555045";

            //            req.Headers.Add("Cookie", "SWID={" + userAuth.ESPNswid + "};" + " espn_s2=" + userAuth.ESPNs2 + " ");
            //            req.Headers.Add("X-Fantasy-Source", "kona");
            //            req.Headers.Add("X-Fantasy-Filter", "{\"players\":{}}");
            //            req.Headers.Add("X-Fantasy-Platform", "kona-PROD-c7fba3b46a9a228b81a5f132e65bd44966a28563");
            //            req.Headers.Add("DNT", "1");
            //            req.Headers.Add("If-None-Match", "02432860d98197a81c050b8a7c4e3ecd7");
            //            req.Headers.Add("Accept-Language", "en-US,en;q=0.5");

            //            req.Timeout = 10000;
            //            req.KeepAlive = true;
            //            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            //            HttpWebResponse response = (HttpWebResponse)req.GetResponse();

            //            response.GetResponseStream();
            //            System.IO.Stream stream = response.GetResponseStream();
            //            System.IO.StreamReader reader = new StreamReader(stream, Encoding.ASCII);
            //            results = reader.ReadToEnd();
            //            reader.Close();
            //            response.Close();
            //        }

            //        return results;
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception("An error occurred reading from ESPN. Please try again.");
            //    }
            //}
            //else
            //    throw new Exception("Make sure your ESPN SWID and S2 are correct.");
        }

        public List<UserLeague> GetLeagues(string swid, Sport sport)
        {
            List<UserLeague> userLeagues = new List<UserLeague>();

            try
            {
                string data = "";
                string url = "https://fan.api.espn.com/apis/v2/fans/{" + CleanSWID(swid) + "}";
                using (var web = new WebClient())
                {
                    try
                    {
                        data = web.DownloadString(url);
                    }
                    catch (Exception ex)
                    {
                        data = "";
                    }
                }

                if (data.Length == 0)
                    return userLeagues;

                JObject rss = JObject.Parse(data);

                foreach (JToken leagueToken in rss["preferences"])
                {
                    if (leagueToken["metaData"]["entry"] == null)
                        continue;

                    string nickname = leagueToken["metaData"]["entry"]["entryMetadata"]["teamName"].ToString();
                    string leagueId = leagueToken["metaData"]["entry"]["groups"].First["groupId"].ToString();
                    string providerId = leagueToken["metaData"]["entry"]["entryId"].ToString();
                    string title = leagueToken["metaData"]["entry"]["groups"].First["groupName"].ToString();

                    string isActive = leagueToken["metaData"]["inSeason"].ToString();
                    string ESPNSport = leagueToken["metaData"]["entry"]["abbrev"].ToString();
                    if (isActive == "false" || ESPNSport != sport.ESPNCode)
                        continue;

                    var userLeague = new UserLeague();
                    userLeague.Title = title;
                    userLeague.ProviderLeagueId = leagueId;
                    userLeague.MyProviderTeamId = providerId;
                    userLeague.MyTeamTitle = nickname;
                    userLeagues.Add(userLeague);
                }
            }
            catch
            {
            }

            return userLeagues;
        }

        public UserLeague ImportUserLeague(
            Sport sport,
            UserAuth userAuth,
            Season season,
            string espnLeagueId,
            List<ActiveRosterSpot> activeRosterSpots,
            List<Category> categories)
        {
            UserLeague league = new UserLeague();
            string data;
            try
            {
                data = ReadESPNUrl(sport, season.ESPNYear, espnLeagueId, userAuth, "mDraftDetail&view=mLiveScoring&view=mMatchupScore&view=mPendingTransactions&view=mPositionalRatings&view=mRoster&view=mSettings&view=mTeam&view=modular&view=mNav");
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to import the ESPN League: " + ex.Message);
            }

            filelib.WriteData(config, "espnleague", userAuth.UserId, espnLeagueId, "json", data);

            JObject rss = JObject.Parse(data);

            if (rss["settings"] == null)
                throw new Exception("The format of the ESPN file is invalid for the given ESPN League ID.");

            league.Title = rss["settings"]["name"].ToString().Trim();
            league.DisplayTitle = league.Title;
            league.UserId = userAuth.UserId;
            league.SeasonId = season.Id;
            league.TrackLeague = true;
            league.ProviderLeagueId = espnLeagueId;
            league.FantasyProviderId = 2;
            league.NumberOfTeams = Convert.ToInt32(rss["settings"]["size"].ToString());
            league.HasDrafted = rss["draftDetail"]["drafted"].ToString() == "true";

            foreach (var myUserLeague in GetLeagues(userAuth.ESPNswid, sport))
            {
                if (myUserLeague.ProviderLeagueId == league.ProviderLeagueId)
                {
                    league.MyTeamTitle = myUserLeague.MyTeamTitle;
                    league.MyProviderTeamId = myUserLeague.MyProviderTeamId;
                }
            }

            string scoring = rss["settings"]["scoringSettings"]["scoringType"].ToString();
            if (scoring == "H2H_POINTS")
            {
                league.ScoringSystem = "P";
                league.LeagueType = "H";
            }
            else if (scoring == "H2H_CATEGORY")
            {
                league.ScoringSystem = "C";
                league.LeagueType = "H";
            }
            else
            {
                league.ScoringSystem = "C";
                league.LeagueType = "R";
            }

            string lineups = rss["settings"]["rosterSettings"]["lineupLocktimeType"].ToString();
            if (lineups == "INDIVIDUAL_FIRSTGAME_WEEKLY")
                league.LineupFrequency = "W";
            else
                league.LineupFrequency = "D";

            if (rss["settings"]["draftSettings"]["type"].ToString() != "SNAKE")
                league.IsAuction = true;

            if (rss["settings"]["draftSettings"]["date"] != null)
            {
                string dateString = rss["settings"]["draftSettings"]["date"].ToString();
                dateString = dateString.Substring(0, dateString.Length - 3);
                long epoch = Convert.ToInt64(dateString);
                DateTime draftDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);
                league.DraftDate = draftDate.AddSeconds(epoch);
            }

            foreach (JProperty rosterToken in rss["settings"]["rosterSettings"]["lineupSlotCounts"])
            {
                var activeRosterSpot = (from ars in activeRosterSpots where ars.ESPNTitle == rosterToken.Name select ars).FirstOrDefault();
                if (activeRosterSpot != null)
                {
                    var userLeagueActiveRosterSpot = new UserLeagueActiveRosterSpot();
                    userLeagueActiveRosterSpot.NumberOfPlayers = Convert.ToInt32(rosterToken.Value);
                    userLeagueActiveRosterSpot.ActiveRosterSpotId = activeRosterSpot.Id;
                    league.UserLeagueActiveRosterSpots.Add(userLeagueActiveRosterSpot);
                    league.PlayersPerTeam += userLeagueActiveRosterSpot.NumberOfPlayers;
                }
                if (sport.IsNBA && rosterToken.Name == "12")   // bench
                    league.PlayersPerTeam += Convert.ToInt32(rosterToken.Value);
                if (sport.IsNFL && rosterToken.Name == "20")   // bench
                    league.PlayersPerTeam += Convert.ToInt32(rosterToken.Value);
                if (sport.IsMLB && rosterToken.Name == "16")   // bench
                    league.PlayersPerTeam += Convert.ToInt32(rosterToken.Value);
            }

            foreach (JObject categoryToken in rss["settings"]["scoringSettings"]["scoringItems"])
            {
                var category = (from c in categories where c.ESPNId == categoryToken["statId"].ToString() select c).FirstOrDefault();
                if (category != null)
                {
                    var userLeagueCategory = new UserLeagueCategory();
                    userLeagueCategory.CategoryId = category.Id;
                    userLeagueCategory.IsActive = true;
                    if (categoryToken["points"] != null && league.ScoringSystem == "P")
                        userLeagueCategory.PointsPerStat = Convert.ToDouble(categoryToken["points"].ToString());
                    league.UserLeagueCategories.Add(userLeagueCategory);
                }
                else
                {
                    if (logger != null)
                        logger.LogError("Missing ESPN StatId " + categoryToken["statId"].ToString() + " " + userAuth.UserId.ToString());
                }
            }

            return league;
        }

        /// <summary>
        /// scoringPeriod names a specific period to read rosters from. Left
        /// null it uses the default, which is fifteen days out for a weekly
        /// league and whatever ESPN considers current for a daily one.
        /// </summary>
        /// <summary>
        /// The players on waivers, onto userLeague.UserLeagueWaiverPlayers.
        ///
        /// ESPN returns free agents and waiver players from the same call and
        /// tells them apart with a status on each entry, so the free agents are
        /// dropped here - a free agent is not on waivers and the wire would be
        /// the whole unowned pool otherwise.
        ///
        /// Players it cannot match are skipped rather than reported. A waiver
        /// list is a convenience rather than a record, unlike a roster where a
        /// missing player means the roster is wrong.
        /// </summary>
        private void ReadESPNWaivers(
            UserAuth userAuth,
            Sport sport,
            Season season,
            UserLeague userLeague,
            List<FantasyProviderPlayer> fantasyProviderPlayers,
            DateTime now)
        {
            string data;

            try
            {
                data = ReadESPNUrl(sport, season.ESPNYear, userLeague.ProviderLeagueId, userAuth,
                    "view=kona_player_info");
            }
            catch
            {
                return;
            }

            if (string.IsNullOrEmpty(data))
                return;

            try
            {
                JObject rss = JObject.Parse(data);
                if (rss["players"] == null)
                    return;

                foreach (JToken entry in rss["players"])
                {
                    // FREEAGENT or WAIVERS. Only the second is the wire.
                    if (entry["status"] == null) continue;
                    if (entry["status"].ToString() != "WAIVERS") continue;

                    if (entry["id"] == null) continue;
                    string espnId = entry["id"].ToString();

                    var providerPlayer = (from pp in fantasyProviderPlayers
                                          where pp.FantasyProvider.Id == 2 && pp.ProviderId == espnId
                                          select pp).FirstOrDefault();

                    if (providerPlayer == null) continue;

                    if (userLeague.UserLeagueWaiverPlayers.Find(w => w.PlayerId == providerPlayer.PlayerId) != null)
                        continue;

                    userLeague.UserLeagueWaiverPlayers.Add(new UserLeagueWaiverPlayer
                    {
                        UserLeagueId = userLeague.Id,
                        PlayerId = providerPlayer.PlayerId,
                        AddedDate = now
                    });
                }
            }
            catch (Exception)
            {
                // A wire we cannot read is not worth failing the roster import
                // for, which is what the caller actually wanted.
            }
        }

        public List<UserLeagueTeam> GetUserLeagueTeams(UserAuth userAuth, Sport sport, Season season, UserLeague userLeague, List<FantasyProviderPlayer> fantasyProviderPlayers, List<Player> allPlayers, List<UserLeagueMissingPlayer> userLeagueMissingPlayers, bool skipWW = false, int? scoringPeriod = null)
        {
            List<UserLeagueTeam> teams = new List<UserLeagueTeam>();
            var now = DateTime.UtcNow;

            string data = "";
            if (userLeague.LineupFrequency == "W")
            {
                // Fifteen days out by default, matching Basketball Monster.
                // A roster change made now may not apply for a while, so
                // today's period is not what the team will actually field.
                int period = scoringPeriod
                    ?? Convert.ToInt32((DateTime.Today - season.StartDate).TotalDays) + RosterDaysAhead;

                data = ReadESPNUrl(sport, season.ESPNYear, userLeague.ProviderLeagueId, userAuth, "mScoreboard&view=mTeam&view=mLiveScoring&view=mMatchupScore&scoringPeriodId=" + period.ToString() + "&view=mRoster");
            }
            else
            {
                try
                {
                    // No period unless one was asked for, which is how this
                    // has always behaved - ESPN then gives the current one.
                    var periodTag = scoringPeriod.HasValue
                        ? "scoringPeriodId=" + scoringPeriod.Value.ToString() + "&"
                        : "";

                    data = ReadESPNUrl(sport, season.ESPNYear, userLeague.ProviderLeagueId, userAuth, periodTag + "rosterForTeamId=" + userLeague.MyProviderTeamId + "&view=mDraftDetail&view=mLiveScoring&view=mMatchupScore&view=mPendingTransactions&view=mPositionalRatings&view=mRoster&view=mSettings&view=mTeam&view=modular&view=mNav");
                }
                catch
                {
                    data = "";
                }
            }

            if (data == "")
                return new List<UserLeagueTeam>();

            filelib.WriteData(config, "espnteams", userAuth.UserId, userLeague.ProviderLeagueId, "json", data);

            List<string> lines = new List<string>();

            bool loadWW = false;

            JObject rss = JObject.Parse(data);

            foreach (JToken teamNode in rss["teams"])
            {
                UserLeagueTeam team = new UserLeagueTeam();
                team.ProviderId = teamNode["id"].ToString();
                team.Title = teamNode["name"].ToString().Trim();
                team.UserLeagueId = userLeague.Id;
                teams.Add(team);

                if (teamNode["roster"] != null)
                {
                    foreach (JToken playerNode in teamNode["roster"]["entries"])
                    {
                        string onTeamId = playerNode["playerPoolEntry"]["onTeamId"].ToString();
                        string fullName = playerNode["playerPoolEntry"]["player"]["fullName"].ToString();
                        string lineupSlot = playerNode["lineupSlotId"].ToString();
                        string espnId = playerNode["playerId"].ToString();
                        var providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == 2 && pp.ProviderId == espnId select pp).FirstOrDefault();
                        if (providerPlayer == null)
                        {
                            var matchPlayers = (from p in allPlayers where p.FirstName + " " + p.LastName == fullName select p).ToList();
                            if (matchPlayers.Count == 1)
                            {
                                providerPlayer = new FantasyProviderPlayer();
                                providerPlayer.PlayerId = matchPlayers.First().Id;
                            }
                            else
                            {
                                string line = espnId + "," + fullName;
                                lines.Add(line);
                            }
                        }
                        if (providerPlayer != null)
                        {
                            UserLeagueTeamPlayer ulp = new UserLeagueTeamPlayer();
                            ulp.PlayerId = providerPlayer.PlayerId;
                            if (sport.IsNBA)
                                ulp.IsActive = (lineupSlot != "12" && lineupSlot != "13");
                            if (sport.IsNFL)
                                ulp.IsActive = (lineupSlot != "20" && lineupSlot != "21");
                            team.UserLeagueTeamPlayers.Add(ulp);
                            loadWW = true;
                        }
                        else
                        {
                            if (userLeagueMissingPlayers != null)
                            {
                                var missingPlayer = new UserLeagueMissingPlayer();
                                missingPlayer.ProviderId = espnId;
                                userLeagueMissingPlayers.Add(missingPlayer);
                            }
                            if (logger != null)
                                logger.LogError("Missing ESPN player " + fullName + " " + espnId);
                        }
                    }
                }
            }

            if (!skipWW)
                ReadESPNWaivers(userAuth, sport, season, userLeague, fantasyProviderPlayers, now);

            // Below is Fantrax code that was copy pasted here and never
            // adapted - it calls fantrax.com and matches on provider id 4, so
            // it would pull the wrong provider's data into an ESPN league.
            // ReadESPNWaivers above is the ESPN version.

            //if (!skipWW && loadWW)
            //{
            //    try
            //    {
            //        url = "https://www.fantrax.com/fxea/general/getLeagueInfo?leagueId=" + userLeague.ProviderLeagueId;
            //        using (var web = new WebClient())
            //            data = web.DownloadString(url);

            //        filelib.WriteData(config, "ftww", userAuth.UserId, userLeague.ProviderLeagueId, "json", data);

            //        rss = JObject.Parse(data);
            //        foreach (JProperty p in rss["playerInfo"])
            //        {
            //            foreach (JToken t in p)
            //            {
            //                if (t["status"].ToString() == "WW")
            //                {
            //                    string fanTraxId = p.Name;
            //                    fanTraxId = fanTraxId.Replace("#1090", ""); // remove for DST
            //                    var providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == 4 && pp.ProviderId == fanTraxId select pp).FirstOrDefault();
            //                    if (providerPlayer != null)
            //                    {
            //                        var waiverPlayer = new UserLeagueWaiverPlayer();
            //                        waiverPlayer.UserLeagueId = userLeague.Id;
            //                        waiverPlayer.PlayerId = providerPlayer.PlayerId;
            //                        waiverPlayer.AddedDate = now;
            //                        if (userLeague.UserLeagueWaiverPlayers.Find(w => w.PlayerId == waiverPlayer.PlayerId) == null)
            //                            userLeague.UserLeagueWaiverPlayers.Add(waiverPlayer);
            //                    }
            //                    else
            //                    {
            //                        var missingPlayer = new UserLeagueMissingPlayer();
            //                        missingPlayer.ProviderId = fanTraxId;
            //                        userLeagueMissingPlayers.Add(missingPlayer);
            //                    }
            //                }
            //            }
            //        }

            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}

            return teams;
        }

        public bool LoginESPN(string username, string password, ref string swid, ref string s2)
        {
            try
            {
                var apiKey = GetESPNAPiKey();
                string data = "{\"loginValue\":\"" + username + "\",\"password\":\"" + password + "\"}";
                var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://registerdisney.go.com/jgc/v6/client/ESPN-ONESITE.WEB-PROD/guest/login?langPref=en-US");
                client.DefaultRequestVersion = new Version(2, 0);
                client.DefaultRequestHeaders.Add("authorization", "APIKEY " + apiKey.APIKey);
                client.DefaultRequestHeaders.Add("referer", "https://cdn.registerdisney.go.com/");
                client.DefaultRequestHeaders.Add("accept-language", "en-US,en;q=0.9");
                client.DefaultRequestHeaders.Add("cache-control", "no-cache");
                // client.DefaultRequestHeaders.Add("conversation-id", "a09a3486-634e-4876-8bd5-b29229d3678e");
                client.DefaultRequestHeaders.Add("correlation-id", apiKey.CorrelationId);
                client.DefaultRequestHeaders.Add("device-id", "null");
                // client.DefaultRequestHeaders.Add("g-recaptcha-v3-response", "03AGdBq27TR_x53dj7HTRBj0OxIiA-sJRrX5sYYrO3AZulzRcFhPwLlTvMRXsneieWzXB6z-kmYY0iQcNeKKyVhjGfksoTaJtsip5NRYqXihFYOPeleW8UynQrAeD2LtZXiJgg60hkAy2uMhe-ipzbMyZrq5lvNZcu5YIWc6iS7maAaDeyr5uzyD1iq7YIRCpCx_RxVdDc04pZErkIo7VuEsKGJJuRtKNoL2R0UZ9SpwCieARy492lLzUxpsEOTTeiLhb8tiACnSroT6gqT7mk0OpbIU8TE_imPWG8xTPaKu0YV-KIzhwUfp0RJ1llXM9cmi2Ui1eiPCNuBx7TJsad3XgliWWgolZAmT4Nlj95tRw3YDcP0kva5CZYpBFFLmTQCOpaKSDwSPzWhaji_QKv6FAOZsZ85KvWBlDKRNloRj8jUTBVtuJwkVbv2gqR_SV97ZqECmHc9lFv6xk-ZslQzPeWDUNThBN18Q");
                client.DefaultRequestHeaders.Add("oneid-reporting", "eyJzb3VyY2UiOiJlc3BuIiwiY29udGV4dCI6ImRpcmVjdCJ9");
                client.DefaultRequestHeaders.Add("origin", "https://cdn.registerdisney.go.com");
                client.DefaultRequestHeaders.Add("pragma", "no-cache");
                client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                client.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");

                var response = client.PostAsync("https://registerdisney.go.com/jgc/v6/client/ESPN-ONESITE.WEB-PROD/guest/login?langPref=en-US", stringContent);
                while (!response.IsCompleted)
                {

                }


                string url = "https://registerdisney.go.com/jgc/v6/client/ESPN-ONESITE.WEB-PROD/guest/login?langPref=en-US";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.CookieContainer = new CookieContainer();
                req.Method = "POST";
                req.ContentLength = buffer.Length;
                string proxy = null;
                req.Proxy = new WebProxy(proxy, true);
                req.Timeout = 10000;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36";

                req.Accept = "*/*";
                req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
                req.KeepAlive = true;
                req.ContentType = "application/json";
                req.Host = "cdn.registerdisney.go.com";
                req.Referer = "https://cdn.registerdisney.go.com/";

                //req.Headers.Add("authority", "registerdisney.go.com");
                //req.Headers.Add("method", "POST");
                //req.Headers.Add("path", "/jgc/v6/client/ESPN-ONESITE.WEB-PROD/guest/login?langPref=en-US");
                //req.Headers.Add("scheme", "https");

                req.Headers.Add("accept-language", "en-US,en;q=0.9");
                req.Headers.Add("authorization", "APIKEY " + apiKey.APIKey);
                req.Headers.Add("cache-control", "no-cache");
                //req.Headers.Add("conversation-id", "a09a3486-634e-4876-8bd5-b29229d3678e");
                req.Headers.Add("correlation-id", apiKey.CorrelationId);
                req.Headers.Add("device-id", "null");
                // req.Headers.Add("expires", "-1");
                //req.Headers.Add("g-recaptcha-v3-response", "03AGdBq27TR_x53dj7HTRBj0OxIiA-sJRrX5sYYrO3AZulzRcFhPwLlTvMRXsneieWzXB6z-kmYY0iQcNeKKyVhjGfksoTaJtsip5NRYqXihFYOPeleW8UynQrAeD2LtZXiJgg60hkAy2uMhe-ipzbMyZrq5lvNZcu5YIWc6iS7maAaDeyr5uzyD1iq7YIRCpCx_RxVdDc04pZErkIo7VuEsKGJJuRtKNoL2R0UZ9SpwCieARy492lLzUxpsEOTTeiLhb8tiACnSroT6gqT7mk0OpbIU8TE_imPWG8xTPaKu0YV-KIzhwUfp0RJ1llXM9cmi2Ui1eiPCNuBx7TJsad3XgliWWgolZAmT4Nlj95tRw3YDcP0kva5CZYpBFFLmTQCOpaKSDwSPzWhaji_QKv6FAOZsZ85KvWBlDKRNloRj8jUTBVtuJwkVbv2gqR_SV97ZqECmHc9lFv6xk-ZslQzPeWDUNThBN18Q");
                req.Headers.Add("oneid-reporting", "eyJzb3VyY2UiOiJlc3BuIiwiY29udGV4dCI6ImRpcmVjdCJ9");
                req.Headers.Add("origin", "https://cdn.registerdisney.go.com");
                req.Headers.Add("pragma", "no-cache");
                req.Headers.Add("sec-fetch-dest", "empty");
                req.Headers.Add("sec-fetch-mode", "cors");
                req.Headers.Add("sec-fetch-site", "same-site");

                Stream reqst = req.GetRequestStream();
                reqst.Write(buffer, 0, buffer.Length);
                reqst.Flush();
                reqst.Close();

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                Stream resst = res.GetResponseStream();
                StreamReader sr = new StreamReader(resst);
                string outString = sr.ReadToEnd();

                JObject rss = JObject.Parse(outString);
                if (rss["data"] != null)
                {
                    swid = (string)rss["data"]["token"]["swid"];
                    s2 = (string)rss["data"]["s2"];
                    swid = swid.Replace("{", "").Replace("}", "");
                    s2 = s2.Replace("{", "").Replace("}", "");

                    return true;
                }
            }
            catch (Exception ex)
            {
                swid = "";
                s2 = "";
            }

            return false;
        }

        private ESPNAPIKey GetESPNAPiKey()
        {
            var key = new ESPNAPIKey();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string url = "https://registerdisney.go.com/jgc/v6/client/ESPN-ONESITE.WEB-PROD/api-key?langPref=en-US";
            url = "https://registerdisney.go.com/jgc/v6/client/ESPN-ONESITE.WEB-PROD/api-key?langPref=en-US";
            string data = "";
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.CookieContainer = new CookieContainer();
            req.Method = "POST";
            req.ContentLength = buffer.Length;
            string proxy = null;
            req.Proxy = new WebProxy(proxy, true);
            req.Timeout = 10000;
            req.Accept = "*/*";
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.KeepAlive = true;

            Stream reqst = req.GetRequestStream();
            reqst.Write(buffer, 0, buffer.Length);
            reqst.Flush();
            reqst.Close();

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream resst = res.GetResponseStream();
            StreamReader sr = new StreamReader(resst);
            key.APIKey = res.Headers["api-key"];
            key.CorrelationId = res.Headers["correlation-id"];

            return key;
        }

        public bool IsConnected(UserAuth userAuth)
        {
            if (userAuth != null && userAuth.ESPNswid != null && userAuth.ESPNs2 != null)
            {
                return userAuth.ESPNswid.Length > 0 && userAuth.ESPNs2.Length > 0;
            }

            return false;
        }

        public async Task<string> ReadWaitUrl(UserAuth userAuth, string url)
        {
            string data = "";

            HttpClient client = new HttpClient();
            var response = await client.GetAsync("https://www.espn.com/");
            var pageContents = await response.Content.ReadAsStringAsync();

            return data;
        }

    }

    public class ESPNAPIKey
    {
        public string APIKey { get; set; }
        public string CorrelationId { get; set; }
    }

}
