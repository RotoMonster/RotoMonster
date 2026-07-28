using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading;

namespace RotoMonster.Core.Libs
{
    public class FanTraxLib
    {
        private readonly IConfiguration config;
        private readonly ILogger logger;
        private FileLib filelib = new FileLib();

        public FanTraxLib(IConfiguration config, ILogger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public bool IsEmailValid(string fanTraxEmail)
        {
            if (fanTraxEmail.Trim().Length > 0)
            {
                string data = GetLeaguesJson(fanTraxEmail);
                if (data.Length > 0)
                {
                    JObject rss = JObject.Parse(data);
                    if (rss["leagues"] != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsConnected(UserAuth userAuth)
        {
            if (userAuth != null && (userAuth.FanTraxEmail != null || userAuth.UserId == ""))
            {
                return userAuth.FanTraxEmail.Length > 0 || userAuth.UserId == "";
            }

            return false;
        }

        void Delay()
        {
             Thread.Sleep(250);
        }

        public string GetLeaguesJson(string fanTraxEmail)
        {
            string data = "";

            try
            {
                string url = "https://www.fantrax.com/fxea/general/getLeagues?userSecretId=" + fanTraxEmail.Trim();
                using (var web = new WebClient())
                {
                    web.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                    data = web.DownloadString(url);
                }

                filelib.WriteData(config, "fleagues", fanTraxEmail, "", "json", data);
            }
            catch
            {
            }

            return data;
        }

        public List<UserLeague> GetLeagues(string leaguesJson, string sport)
        {
            List<UserLeague> userLeagues = new List<UserLeague>();

            try
            {
                JObject rss = JObject.Parse(leaguesJson);
                if (rss["leagues"] != null)
                {
                    foreach (JToken token in rss["leagues"])
                    {
                        if ((string)token["sport"] == sport)
                        {
                            UserLeague userLeague = new UserLeague();
                            userLeague.Title = (string)token["leagueName"];
                            userLeague.ProviderLeagueId = (string)token["leagueId"];
                            userLeague.MyTeamTitle = (string)token["teamName"];
                            userLeague.MyProviderTeamId = (string)token["teamId"];
                            userLeagues.Add(userLeague);
                        }
                    }
                }
            }
            catch
            {

            }

            return userLeagues;
        }

        public UserLeague ImportUserLeague(
            UserAuth userAuth,
            Season season,
            string fanTraxLeagueId,
            string leagueTitle,
            List<ActiveRosterSpot> activeRosterSpots,
            List<Category> categories)
        {
            string url = "https://www.fantrax.com/fxea/general/getLeagueInfo?leagueId=" + fanTraxLeagueId;
            string data = "";
            try
            {
                using (var web = new WebClient())
                {
                    web.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                    data = web.DownloadString(url);
                }
            }
            catch
            {
                return null;
            }

            filelib.WriteData(config, "fleague", userAuth.UserId, fanTraxLeagueId, "json", data);

            JObject rss = JObject.Parse(data);
            if (rss["rosterInfo"] == null)
                return null;

            UserLeague league = new UserLeague();
            league.UserId = userAuth.UserId;
            league.SeasonId = season.Id;
            league.TrackLeague = true;
            league.ProviderLeagueId = fanTraxLeagueId;
            league.FantasyProviderId = 4;
            league.LineupFrequency = "W";
            league.Title = leagueTitle;
            league.IsProLeague = league.Title.Contains("Classic Draft");
            league.NumberOfTeams = rss["teamInfo"].Count();
            league.PlayersPerTeam = Convert.ToInt32(rss["rosterInfo"]["maxTotalPlayers"]);
            league.DisplayTitle = league.Title = "FanTrax " + fanTraxLeagueId;

            foreach (JProperty p in rss["rosterInfo"]["positionConstraints"])
            {
                string posText = p.Name;
                if (posText == "Flx")
                    posText = "Util";
                UserLeagueActiveRosterSpot rs = new UserLeagueActiveRosterSpot();
                rs.NumberOfPlayers = Convert.ToInt32(p.Value["maxActive"]);
                var ars = (from a in activeRosterSpots where a.Title == posText || a.FanTraxTitle == posText select a).FirstOrDefault();
                if (ars != null)
                {
                    rs.ActiveRosterSpotId = ars.Id;
                    league.UserLeagueActiveRosterSpots.Add(rs);
                }
                else
                {
                    league.AddError("No match for active roster spot " + posText);
                }
            }

            string scoringType = ((string)rss["scoringSystem"]["type"]).ToLower();
            if (scoringType == "rotisserie")
            {
                league.ScoringSystem = "C";
                league.LeagueType = "R";
            }
            else if (scoringType == "points")
            {
                league.ScoringSystem = "P";
                league.LeagueType = "H";
            }
            else
            {
                throw new Exception("Unknown scoringType " + scoringType + " " + fanTraxLeagueId);
            }

            CategoryLib catLib = new CategoryLib();
            foreach (var p1 in rss["scoringSystem"]["scoringCategorySettings"])
            {
                string fanTraxGroup = p1["group"]["code"].ToString();
                foreach (var p2 in p1)
                {
                    foreach (var p in p2)
                    {
                        foreach (var p3 in p)
                        {
                            if (p3.Count() > 1)
                            {
                                string fanTraxCatId = (string)p3["scoringCategory"]["id"];
                                string fanTraxCode = (string)p3["scoringCategory"]["code"];

                                if (fanTraxCode == "TEAM_POINTS_AGAINST_TEAM" || fanTraxCode == "TEAM_POINTS_AGAINST_DEFENSE")  // custom code for NFL
                                {
                                    if (p3["ranges"] != null)
                                    {
                                        foreach (var r1 in p3["ranges"])
                                        {
                                            var rangeStart = r1["range"]["start"].ToString();
                                            var rangeEnd = r1["range"]["end"].ToString();
                                            var points = r1["points"].ToString();

                                            var findCat = "pts" + rangeStart;
                                            if (rangeEnd != "0")
                                                findCat += "to" + rangeEnd;
                                            var rangeCat = (from c in categories where c.FanTraxGroup == fanTraxGroup && c.Abbreviation == findCat select c).FirstOrDefault();
                                            if (rangeCat != null)
                                            {
                                                var ulcRange = new UserLeagueCategory();
                                                ulcRange.IsActive = true;
                                                ulcRange.CategoryId = rangeCat.Id;
                                                ulcRange.PointsPerStat = Convert.ToDouble(points);
                                                league.UserLeagueCategories.Add(ulcRange);
                                            }
                                            else
                                            {
                                                league.AddError("No match for range category " + fanTraxCatId + " " + fanTraxCode + " " + rangeStart + " to " + rangeEnd);
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    var cats = catLib.GetFanTraxCategories(categories, fanTraxGroup, fanTraxCatId);
                                    if (cats.Count > 0)
                                    {
                                        foreach (var cat in cats)
                                        {
                                            UserLeagueCategory ulc = new UserLeagueCategory();
                                            ulc.IsActive = true;
                                            ulc.CategoryId = cat.Id;
                                            string valueText = (string)p3["points"];
                                            if (league.ScoringSystem == "C")
                                            {
                                                ulc.Weight = Convert.ToDouble(valueText);
                                                if (fanTraxCode == "INDIVIDUAL_HOME_RUNS" && ulc.Weight < 0)
                                                    ulc.CategoryId = 73;
                                            }
                                            else
                                                ulc.PointsPerStat = Convert.ToDouble(valueText);

                                            if (league.UserLeagueCategories.Find(c => c.CategoryId == ulc.CategoryId) == null)
                                            {
                                                league.UserLeagueCategories.Add(ulc);
                                            }
                                            else
                                            {
                                                league.AddError("Attempt to add category twice " + fanTraxGroup + " " + fanTraxCatId + " " + fanTraxCode);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        league.AddError("No match for category " + fanTraxGroup + " " + fanTraxCatId + " " + fanTraxCode);
                                    }

                                }

                            }
                        }
                    }
                }
            }

            return league;
        }

        public int GetCurrentFanTraxPeriod(string fanTraxLeagueId)
        {
            int period = 0;
            string url = "https://www.fantrax.com/fxea/general/getTeamRosters?leagueId=" + fanTraxLeagueId;
            string data = "";

            using (var web = new WebClient())
            {
                web.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                data = web.DownloadString(url);
            }

            try
            {
                JObject rss = JObject.Parse(data);
                if (rss["period"] != null)
                {
                    period = Convert.ToInt32(rss["period"].ToString());
                }
            }
            catch (Exception)
            {
            }

            return period;
        }

        public List<UserLeagueTeam> GetUserLeagueTeams(UserAuth userAuth, string sport, UserLeague userLeague, List<FantasyProviderPlayer> fantasyProviderPlayers, List<UserLeagueMissingPlayer> userLeagueMissingPlayers, bool skipWW = false)
        {
            List<UserLeagueTeam> teams = new List<UserLeagueTeam>();
            var now = DateTime.UtcNow;

            //if (userLeague.MyProviderTeamId.Length == 0)
            //{
            //    var allLeagues = GetLeagues(GetLeaguesJson(userAuth.FanTraxEmail), sport);
            //    var thisLeague = (from l in allLeagues where l.ProviderLeagueId == userLeague.ProviderLeagueId select l).FirstOrDefault();

            //    if (thisLeague != null)
            //    {
            //        userLeague.MyProviderTeamId = thisLeague.MyProviderTeamId;
            //        userLeague.MyTeamTitle = thisLeague.MyTeamTitle;
            //        userLeague.Title = thisLeague.Title;
            //        if (userLeague.DisplayTitle == null)
            //            userLeague.DisplayTitle = userLeague.Title;
            //    }
            //}

            string url = "https://www.fantrax.com/fxea/general/getTeamRosters?leagueId=" + userLeague.ProviderLeagueId + "&period=" + (GetCurrentFanTraxPeriod(userLeague.ProviderLeagueId)+1).ToString();
            string data = "";
            try
            {
                using (var web = new WebClient())
                {
                    web.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                    data = web.DownloadString(url);
                }
            }
            catch
            {
                data = "";
            }

            filelib.WriteData(config, "fteams", userAuth.UserId, userLeague.ProviderLeagueId, "json", data);

            if (data == "")
                return new List<UserLeagueTeam>();

            bool loadWW = false;

            JObject rss = JObject.Parse(data);
            foreach(var node in rss)
            {
                if (node.Key == "rosters")
                {
                    foreach(JProperty p in node.Value)
                    {
                        UserLeagueTeam team = new UserLeagueTeam();
                        team.ProviderId = p.Name;
                        team.Title = (string)p.Value["teamName"];
                        team.UserLeagueId = userLeague.Id;
                        teams.Add(team);
                        if (p.Value["rosterItems"] != null)
                        {
                            foreach (var pNode in p.Value["rosterItems"])
                            {
                                string fanTraxId = (string)pNode["id"];
                                string status = (string)pNode["status"];
                                string position = (string)pNode["position"];

                                var providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == 4 && pp.ProviderId == fanTraxId select pp).FirstOrDefault();
                                if (providerPlayer != null)
                                {
                                    UserLeagueTeamPlayer ulp = new UserLeagueTeamPlayer();
                                    ulp.PlayerId = providerPlayer.Player.Id;
                                    ulp.IsActive = (status == "ACTIVE");
                                    team.UserLeagueTeamPlayers.Add(ulp);
                                    loadWW = true;

                                    if ((string)config["sport"].ToLower() == "mlb")
                                    {
                                        if (providerPlayer.Player.Id == 12251)   // if Ohtani hitter, then add pitcher too
                                        {
                                            ulp = new UserLeagueTeamPlayer();
                                            ulp.PlayerId = 12214;
                                            ulp.IsActive = (status == "ACTIVE");
                                            team.UserLeagueTeamPlayers.Add(ulp);
                                        }
                                    }
                                }
                                else
                                {
                                    if (userLeagueMissingPlayers != null)
                                    {
                                        var missingPlayer = new UserLeagueMissingPlayer();
                                        missingPlayer.ProviderId = fanTraxId;
                                        userLeagueMissingPlayers.Add(missingPlayer);
                                    }
                                    if (logger != null)
                                        logger.LogError("Missing FanTrax player " + fanTraxId);
                                }
                            }
                        }
                    }
                }
            }

            if(!skipWW)
            //if (!skipWW && loadWW)
            {
                try
                {
                    Delay();
                    url = "https://www.fantrax.com/fxea/general/getLeagueInfo?leagueId=" + userLeague.ProviderLeagueId;
                    using (var web = new WebClient())
                    {
                        web.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                        data = web.DownloadString(url);
                    }

                    filelib.WriteData(config, "ftww", userAuth.UserId, userLeague.ProviderLeagueId, "json", data);

                    rss = JObject.Parse(data);
                    if (rss["playerInfo"] != null)
                    {
                        foreach (JProperty p in rss["playerInfo"])
                        {
                            foreach (JToken t in p)
                            {
                                if (t["status"].ToString() == "WW")
                                {
                                    string fanTraxId = p.Name;
                                    fanTraxId = fanTraxId.Replace("#1090", ""); // remove for DST
                                    var providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == 4 && pp.ProviderId == fanTraxId select pp).FirstOrDefault();
                                    if (providerPlayer != null)
                                    {
                                        var waiverPlayer = new UserLeagueWaiverPlayer();
                                        waiverPlayer.UserLeagueId = userLeague.Id;
                                        waiverPlayer.PlayerId = providerPlayer.PlayerId;
                                        waiverPlayer.AddedDate = now;
                                        if (userLeague.UserLeagueWaiverPlayers.Find(w => w.PlayerId == waiverPlayer.PlayerId) == null)
                                            userLeague.UserLeagueWaiverPlayers.Add(waiverPlayer);
                                    }
                                    else
                                    {
                                        var missingPlayer = new UserLeagueMissingPlayer();
                                        missingPlayer.ProviderId = fanTraxId;
                                        userLeagueMissingPlayers.Add(missingPlayer);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return teams;
        }

        public DateTime GetDraftDate(
            UserAuth userAuth,
            string fanTraxLeagueId,
            DateTime defaultDraftDate
            )
        {
            if (!IsConnected(userAuth))
                return defaultDraftDate;

            string url = "https://www.fantrax.com/fxea/general/getDraftResults?leagueId=" + fanTraxLeagueId;
            string data = "";
            using (var web = new WebClient())
            {
                web.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                data = web.DownloadString(url);
            }

            JObject rss = JObject.Parse(data);
            if (rss["draftPicks"] != null)
            {
                DateTime draftDate = DateTime.MaxValue;
                foreach (JToken p in rss["draftPicks"].Children())
                {
                    if (p["time"] != null)
                    {
                        long epoch = Convert.ToInt64(p["time"]);
                        DateTime currentDraftDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);
                        currentDraftDate = currentDraftDate.AddMilliseconds(epoch);
                        if (currentDraftDate < draftDate)
                            draftDate = currentDraftDate;
                    }
                }

                if (draftDate != DateTime.MaxValue)
                    return draftDate;
            }

            return defaultDraftDate;
        }

        public Draft ImportDraft(
            UserAuth userAuth,
            UserLeague userLeague,
            List<FantasyProviderPlayer> fantasyProviderPlayers
            )
        {
            Draft draft = null;

            //if (!IsConnected(userAuth))
            //    return draft;

            string url = "https://www.fantrax.com/fxea/general/getDraftResults?leagueId=" + userLeague.ProviderLeagueId;
            string data = "";
            using (var web = new WebClient())
            {
                web.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                data = web.DownloadString(url);
            }

            if (userAuth != null)
                filelib.WriteData(config, "fdraft", userAuth.UserId, userLeague.ProviderLeagueId, "json", data);

            JObject rss = JObject.Parse(data);
            if (rss["draftPicks"] == null)
                return null;

            draft = new Draft();
            draft.DraftPlayers = new List<DraftPlayer>();
            draft.ProviderLeagueId = userLeague.ProviderLeagueId;
            if (rss["startDate"] == null)
                draft.DraftDate = userLeague.DraftDate.GetValueOrDefault();
            else
                draft.DraftDate = Convert.ToDateTime(rss["startDate"]);
            draft.NumberOfTeams = userLeague.NumberOfTeams;
            draft.LeagueSize = userLeague.NumberOfTeams * userLeague.PlayersPerTeam;
            draft.IsMoney = userLeague.IsMoney;
            draft.FantasyProviderId = userLeague.FantasyProviderId;
            draft.LeagueType = userLeague.LeagueType;
            draft.Title = userLeague.Title;
            draft.IsAuction = ((string)rss["draftType"] == "auction");
            draft.SeasonId = userLeague.SeasonId;
            draft.IsFinished = ((string)rss["draftState"] == "completed");
            draft.IsLive = ((string)rss["draftState"] == "live");
            draft.ImportUserLeague(userLeague);
            draft.IsProLeague = draft.Title != null ? (draft.Title.Contains("Classic Draft")) : false;

            foreach (JToken p in rss["draftPicks"].Children())
            {
                if (p["teamId"] == null || p["playerId"] == null || (string)p["playerId"] == "null")
                    continue;

                string playerKey = (string)p["playerId"];
                var providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == userLeague.FantasyProviderId && pp.ProviderId == playerKey select pp).FirstOrDefault();
                if (providerPlayer != null)
                {
                    DraftPlayer draftPlayer = new DraftPlayer();
                    draftPlayer.PlayerId = providerPlayer.Player.Id;
                    draftPlayer.ProviderTeamId = (string)p["teamId"];
                    if (p["pick"] != null)
                        draftPlayer.DraftOrder = Convert.ToInt32(p["pick"]);
                    if (p["bid"] != null)
                        draftPlayer.Price = Convert.ToInt32(p["bid"]);
                    var existing = (from dp in draft.DraftPlayers where dp.PlayerId == draftPlayer.PlayerId select dp).FirstOrDefault();
                    if (existing == null)
                    {
                        draft.DraftPlayers.Add(draftPlayer);
                    }
                    else
                    {
                        if (logger != null)
                        {
                            logger.LogError("Duplicate player in FanTrax draft " + userLeague.ProviderLeagueId);
                            // throw new Exception("Duplicate player in FanTrax draft " + userLeague.ProviderLeagueId);
                        }
                    }
                }
                else
                {
                    if (logger != null)
                        logger.LogError("Missing FanTrax player " + playerKey);
                }

            }

            return draft;
        }


    }
}
