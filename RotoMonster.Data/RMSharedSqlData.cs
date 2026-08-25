using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonster.Data;
using RotoMonsterExternalAPIs.Client.Models;
using RotoMonsterExternalAPIs.Client.Services.Yahoo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;

namespace RotoMonster.Data
{
    public class RMSharedSqlData : IRMSharedData, IYahooTokenStore
    {
        private readonly RMSharedDbContext db;
        private readonly IConfiguration config;
        private readonly IMemoryCache memoryCache;

        public List<UserAuth> UserAuths
        {
            get
            {
                return db.UserAuths.ToList();
            }
        }

        public RMSharedSqlData(RMSharedDbContext db, IConfiguration config, IMemoryCache memoryCache)
        {
            this.db = db;
            this.config = config;
            this.memoryCache = memoryCache;
        }

        private UserAuth GetNewUserAuth(string userId)
        {
            var newAuth = new UserAuth();
            newAuth.UserId = userId;
            newAuth.DateAdded = DateTime.UtcNow;
            newAuth.LastUsed = DateTime.UtcNow;

            return newAuth;
        }

        public UserAuth AddESPNUserAuth(string userId, string swid, string s2)
        {
            UserAuth userAuth = GetUserAuth(userId);
            if (userAuth == null)
            {
                userAuth = GetNewUserAuth(userId);
                userAuth.ESPNswid = swid;
                userAuth.ESPNs2 = s2;
                db.UserAuths.Add(userAuth);
            }
            else
            {
                userAuth.ESPNswid = swid;
                userAuth.ESPNs2 = s2;
                userAuth.LastUsed = DateTime.UtcNow;
            }
            db.SaveChanges();

            return userAuth;
        }

        public UserAuth AddYahooUserAuth(string userId, string accessToken, string refreshToken)
        {
            UserAuth userAuth = GetUserAuth(userId);
            if (userAuth == null)
            {
                userAuth = GetNewUserAuth(userId);
                userAuth.YahooAccessToken = accessToken;
                userAuth.YahooRefreshToken = refreshToken;
                db.UserAuths.Add(userAuth);
            }
            else
            {
                userAuth.YahooAccessToken = accessToken;
                userAuth.YahooRefreshToken = refreshToken;
                userAuth.LastUsed = DateTime.UtcNow;
            }

            db.SaveChanges();

            return userAuth;
        }

        public List<ApplicationUser> GetUsers()
        {
            return db.Users.OrderBy(u => u.Email).ToList();
        }

        public UserAuth GetUserAuth(string userId)
        {
            var auth = (from ua in db.UserAuths where ua.UserId == userId select ua).FirstOrDefault();

            return auth;
        }

        public UserAuth AddUserAuth(UserAuth userAuth)
        {
            userAuth.DateAdded = DateTime.UtcNow;
            userAuth.LastUsed = userAuth.DateAdded;
            db.UserAuths.Add(userAuth);
            db.SaveChanges();

            return userAuth;
        }

        public UserAuth AddFanTraxUserAuth(string userId, string fanTraxEmail)
        {
            UserAuth userAuth = GetUserAuth(userId);
            if (userAuth == null)
            {
                userAuth = GetNewUserAuth(userId);
                userAuth.FanTraxEmail = fanTraxEmail;
                db.UserAuths.Add(userAuth);
            }
            else
            {
                userAuth.FanTraxEmail = fanTraxEmail;
                userAuth.LastUsed = DateTime.UtcNow;
            }
            db.SaveChanges();

            return userAuth;
        }

        /// <summary>
        /// Both values are stored. The id is what every Sleeper call uses,
        /// the name is only so it can be shown back to the user.
        /// </summary>
        public UserAuth AddSleeperUserAuth(string userId, string sleeperName, string sleeperId)
        {
            UserAuth userAuth = GetUserAuth(userId);
            if (userAuth == null)
            {
                userAuth = GetNewUserAuth(userId);
                userAuth.SleeperName = sleeperName;
                userAuth.SleeperId = sleeperId;
                db.UserAuths.Add(userAuth);
            }
            else
            {
                userAuth.SleeperName = sleeperName;
                userAuth.SleeperId = sleeperId;
                userAuth.LastUsed = DateTime.UtcNow;
            }
            db.SaveChanges();

            return userAuth;
        }

        public UserAuth AddCBSUserAuth(string userId, string cbsPid)
        {
            UserAuth userAuth = GetUserAuth(userId);
            if (userAuth == null)
            {
                userAuth = GetNewUserAuth(userId);
                userAuth.CBSPid = cbsPid;
                db.UserAuths.Add(userAuth);
            }
            else
            {
                userAuth.CBSPid = cbsPid;
                userAuth.LastUsed = DateTime.UtcNow;
            }
            db.SaveChanges();

            return userAuth;
        }

        public void ClearCBSAuth(string userId)
        {
            UserAuth auth = GetUserAuth(userId);
            if (auth != null)
            {
                auth.CBSPid = null;
                db.SaveChanges();
            }
        }

        public void ClearSleeperAuth(string userId)
        {
            UserAuth auth = GetUserAuth(userId);
            if (auth != null)
            {
                auth.SleeperName = null;
                auth.SleeperId = null;
                db.SaveChanges();
            }
        }

        public void ClearYahooAuth(string userId)
        {
            UserAuth auth = GetUserAuth(userId);
            if (auth != null)
            {
                auth.YahooAccessToken = null;
                auth.YahooRefreshToken = null;
                db.SaveChanges();
            }
        }

        public void ClearESPNAuth(string userId)
        {
            UserAuth auth = GetUserAuth(userId);
            if (auth != null)
            {
                auth.ESPNswid = null;
                auth.ESPNs2 = null;
                db.SaveChanges();
            }
        }

        public void ClearFanTraxAuth(string userId)
        {
            UserAuth auth = GetUserAuth(userId);
            if (auth != null)
            {
                auth.FanTraxEmail = null;
                db.SaveChanges();
            }
        }

        // Yahoo Calls
        public string GetYahooAPIXML(
            UserAuth userAuth,
            string url)
        {
            var client = new YahooApiClient(
                new YahooOAuth(config["YahooClientId"], config["YahooClientSecret"]),
                this);

            var result = client.GetAsync(userAuth.UserId, url).GetAwaiter().GetResult();

            // Returning "" on failure to match what the old version did - every
            // caller checks the length rather than expecting an exception.
            return result.Success ? result.Content : "";
        }

        // ---- IYahooTokenStore ----
        // Tokens live on UserAuth. There is no expiry column, so it is derived
        // from LastUsed the same way MustRefreshYahoo already does, which keeps
        // this from needing a migration.

        Task<YahooTokens> IYahooTokenStore.LoadAsync(string userKey)
        {
            var auth = GetUserAuth(userKey);

            if (auth == null || string.IsNullOrEmpty(auth.YahooRefreshToken))
                return Task.FromResult<YahooTokens>(null);

            return Task.FromResult(new YahooTokens
            {
                AccessToken = auth.YahooAccessToken,
                RefreshToken = auth.YahooRefreshToken,
                ExpiresAtUtc = auth.LastUsed.AddMinutes(55)
            });
        }

        Task IYahooTokenStore.SaveAsync(string userKey, YahooTokens tokens)
        {
            AddYahooUserAuth(userKey, tokens.AccessToken, tokens.RefreshToken);
            return Task.CompletedTask;
        }

        public string GetLeaguesXml(UserAuth userAuth, string yahooSeasonId)
        {
            string url = "https://fantasysports.yahooapis.com/fantasy/v2/users;use_login=1/games;game_keys=" + yahooSeasonId + "/leagues";
            string data = GetYahooAPIXML(userAuth, url);

            FileLib filelib = new FileLib();
            filelib.WriteData(config, "yleagues", userAuth.UserId, yahooSeasonId, "xml", data);

            return data;
        }

        public List<UserLeague> GetLeagues(string leaguesXml)
        {
            List<UserLeague> userLeagues = new List<UserLeague>();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(leaguesXml);

            if (xml["fantasy_content"] != null)
            {
                foreach (XmlNode userNode in xml["fantasy_content"]["users"].ChildNodes)
                {
                    foreach (XmlNode gameNode in userNode["games"].ChildNodes)
                    {
                        foreach (XmlNode leagueNode in gameNode["leagues"].ChildNodes)
                        {
                            UserLeague league = new UserLeague();
                            league.ProviderLeagueId = leagueNode["league_id"].InnerText;
                            league.Title = leagueNode["name"].InnerText;
                            league.NumberOfTeams = Convert.ToInt32(leagueNode["num_teams"].InnerText);
                            if (leagueNode["scoring_type"].InnerText == "head")
                                league.LeagueType = "H";
                            else if (leagueNode["scoring_type"].InnerText == "roto")
                                league.LeagueType = "R";
                            userLeagues.Add(league);
                        }
                    }
                }
            }

            return userLeagues;
        }

        public List<UserLeagueTeam> GetUserLeagueTeams(
            UserAuth userAuth,
            string yahooSeasonId,
            UserLeague userLeague,
            List<FantasyProviderPlayer> fantasyProviderPlayers,
            List<UserLeagueMissingPlayer> userLeagueMissingPlayers,
            ILogger logger,
            XmlDocument inXml = null)
        {
            List<UserLeagueTeam> teams = new List<UserLeagueTeam>();

            string data = "";
            if (inXml == null)
            {
                DateTime rosterDate = DateTime.Today.AddDays(7);
                string url = "https://fantasysports.yahooapis.com/fantasy/v2/league/" + yahooSeasonId + ".l." + userLeague.ProviderLeagueId + "/teams/roster/players";
                url += String.Format(";date={0}-{1:00}-{2:00}", rosterDate.Year, rosterDate.Month, rosterDate.Day);

                data = GetYahooAPIXML(userAuth, url);
                FileLib filelib = new FileLib();
                filelib.WriteData(config, "yteams", userAuth.UserId, userLeague.ProviderLeagueId, "xml", data);
            }

            try
            {
                XmlDocument xml = inXml;
                if (inXml == null)
                {
                    xml = new XmlDocument();
                    xml.LoadXml(data);
                }
                if (xml["fantasy_content"] != null)
                {
                    foreach (XmlNode teamNode in xml["fantasy_content"]["league"]["teams"].ChildNodes)
                    {
                        string teamKey = teamNode["team_id"].InnerText;
                        UserLeagueTeam team = team = new UserLeagueTeam();
                        team.UserLeagueId = userLeague.Id;
                        team.ProviderId = teamKey;
                        team.Title = teamNode["name"].InnerText;
                        XmlNode managersNode = teamNode["manager"];
                        team.DraftOrder = Convert.ToInt32(teamKey);
                        teams.Add(team);

                        foreach (XmlNode managerNode in teamNode["managers"].ChildNodes)
                        {
                            if (managerNode["is_current_login"] != null && managerNode["is_current_login"].InnerText == "1")
                            {
                                userLeague.MyProviderTeamId = team.ProviderId;
                                userLeague.MyTeamTitle = team.Title;
                            }
                        }

                        foreach (XmlNode playerNode in teamNode["roster"]["players"].ChildNodes)
                        {
                            string yahooId = playerNode["player_id"].InnerText;
                            string fullName = playerNode["name"]["full"].InnerText;
                            string positionType = playerNode["position_type"].InnerText;
                            var providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == 1 && pp.ProviderId == yahooId select pp).FirstOrDefault();
                            if (providerPlayer == null && fullName == "Shohei Ohtani (Pitcher)")
                                providerPlayer = (from pp in fantasyProviderPlayers where pp.PlayerId == 12214 select pp).FirstOrDefault();
                            if (providerPlayer == null && fullName == "Shohei Ohtani (Batter)")
                                providerPlayer = (from pp in fantasyProviderPlayers where pp.PlayerId == 12251 select pp).FirstOrDefault();
                            if (providerPlayer == null)
                            {
                                string editorialPlayerKey = playerNode["editorial_player_key"].InnerText;
                                var items = editorialPlayerKey.Split(".");
                                yahooId = items[items.Length - 1];
                                providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == 1 && pp.ProviderId == yahooId + positionType select pp).FirstOrDefault();
                            }

                            if (providerPlayer != null)
                            {
                                UserLeagueTeamPlayer tp = new UserLeagueTeamPlayer();
                                tp.PlayerId = providerPlayer.Player.Id;
                                string rosterSpot = playerNode["selected_position"]["position"].InnerText;
                                tp.IsIR = (rosterSpot == "IL" || rosterSpot == "IR");
                                tp.IsActive = !tp.IsIR && rosterSpot != "BN";
                                team.UserLeagueTeamPlayers.Add(tp);
                            }
                            else
                            {
                                if (userLeagueMissingPlayers != null)
                                {
                                    var missingPlayer = new UserLeagueMissingPlayer();
                                    missingPlayer.ProviderId = yahooId + "," + fullName;
                                    userLeagueMissingPlayers.Add(missingPlayer);
                                }

                                if (logger != null)
                                    logger.LogError("Missing Yahoo player " + fullName + " " + yahooId + " for league " + userLeague.ProviderLeagueId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return teams;
        }


        public UserLeague ImportUserLeague(
            UserAuth userAuth,
            Season season,
            string yahooLeagueId,
            List<ActiveRosterSpot> activeRosterSpots,
            List<Category> categories,
            ILogger logger)
        {
            UserLeague league = null;

            string url = "https://fantasysports.yahooapis.com/fantasy/v2/league/" + season.YahooId + ".l." + yahooLeagueId + "/settings";

            string data = GetYahooAPIXML(userAuth, url);

            FileLib filelib = new FileLib();
            filelib.WriteData(config, "yleague", userAuth.UserId, yahooLeagueId, "xml", data);

            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(data);

                if (xml["fantasy_content"] != null)
                {
                    league = new UserLeague();
                    league.UserId = userAuth.UserId;
                    league.SeasonId = season.Id;
                    league.TrackLeague = true;
                    league.ProviderLeagueId = yahooLeagueId;
                    league.FantasyProviderId = 1;

                    XmlNode leagueNode = xml["fantasy_content"]["league"];

                    league.HasDrafted = (leagueNode["draft_status"].InnerText == "postdraft");

                    XmlNode sNode = leagueNode["settings"];
                    if (leagueNode["settings"]["draft_time"] != null)
                    {
                        long epoch = Convert.ToInt64(leagueNode["settings"]["draft_time"].InnerText);
                        DateTime draftDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);
                        draftDate = draftDate.AddSeconds(epoch);
                        league.DraftDate = draftDate;
                    }
                    league.Title = leagueNode["name"].InnerText;
                    league.DisplayTitle = league.Title;
                    league.IsMoney = (leagueNode["is_cash_league"].InnerText == "1");
                    league.IsProLeague = (leagueNode["is_pro_league"].InnerText == "1");
                    league.IsAuction = (sNode["is_auction_draft"].InnerText == "1");
                    league.NumberOfTeams = sNode["max_teams"] == null ? Convert.ToInt32(leagueNode["num_teams"].InnerText) : Convert.ToInt32(sNode["max_teams"].InnerText);
                    league.LeagueType = (leagueNode["scoring_type"].InnerText == "head") ? "H" : "R";
                    league.ContinuousWaivers = (sNode["waiver_rule"] != null && sNode["waiver_rule"].InnerText == "continuous");
                    league.WaiverType = (sNode["waiver_type"] != null ? sNode["waiver_type"].InnerText : "");
                    league.WaiverRule = (sNode["waiver_rule"] != null ? sNode["waiver_rule"].InnerText : "");
                    if (leagueNode["weekly_deadline"].InnerText == "1")
                    {
                        league.LineupFrequency = "W";
                        league.SameDayTransactions = true;
                    }
                    else
                    {
                        league.LineupFrequency = "D";
                        league.SameDayTransactions = (leagueNode["weekly_deadline"].InnerText == "intraday");
                    }
                    if (leagueNode["scoring_type"] != null && leagueNode["scoring_type"].InnerText == "point")
                        league.ScoringSystem = "P";
                    else
                        league.ScoringSystem = "C";

                    if (sNode["uses_playoff"].InnerText == "1")
                    {

                    }

                    if (sNode["roster_positions"] != null)
                    {
                        foreach (XmlNode posNode in sNode["roster_positions"].ChildNodes)
                        {
                            string posText = posNode["position"].InnerText;
                            if (posText == "IL" || posText == "IR" || posText == "DL")
                            {
                                league.IRSpots = Convert.ToInt32(posNode["count"].InnerText);
                                continue;
                            }

                            if (posText == "BN")
                            {
                                league.PlayersPerTeam += Convert.ToInt32(posNode["count"].InnerText);
                            }
                            else
                            {
                                UserLeagueActiveRosterSpot rs = new UserLeagueActiveRosterSpot();
                                rs.NumberOfPlayers = Convert.ToInt32(posNode["count"].InnerText);
                                league.PlayersPerTeam += rs.NumberOfPlayers;
                                var ars = (from a in activeRosterSpots where a.Title == posText || a.YahooTitle == posText select a).FirstOrDefault();
                                if (ars != null)
                                {
                                    rs.ActiveRosterSpotId = ars.Id;
                                    league.UserLeagueActiveRosterSpots.Add(rs);
                                }
                                else
                                {
                                    if (logger != null)
                                        logger.LogError("No match for active roster spot " + posText);
                                }

                            }
                        }

                        if (sNode["stat_categories"] != null && sNode["stat_categories"]["stats"] != null)
                        {
                            foreach (XmlNode statNode in sNode["stat_categories"]["stats"].ChildNodes)
                            {
                                UserLeagueCategory ulc = new UserLeagueCategory();
                                ulc.IsActive = true;
                                if (statNode["is_only_display_stat"] != null && statNode["is_only_display_stat"].InnerText == "1")
                                    continue;
                                string statId = statNode["stat_id"].InnerText;
                                string statName = statNode["name"].InnerText;
                                string positionType = statNode["position_type"].InnerText;
                                if (positionType == "DP")
                                    continue;

                                var cat = (from c in categories where c.YahooId == statId select c).FirstOrDefault();
                                if (cat != null)
                                {
                                    ulc.CategoryId = cat.Id;
                                    league.UserLeagueCategories.Add(ulc);
                                }
                                else
                                {
                                    if (logger != null)
                                        logger.LogError("No match for category " + statId);
                                }
                            }
                        }

                        if (sNode["stat_modifiers"] != null && sNode["stat_modifiers"]["stats"] != null)
                        {
                            league.ScoringSystem = "P";
                            foreach (XmlNode statNode in sNode["stat_modifiers"]["stats"].ChildNodes)
                            {
                                string statId = statNode["stat_id"].InnerText;
                                var cat = (from c in categories where c.YahooId == statId select c).FirstOrDefault();
                                if (cat != null)
                                {
                                    var ulc = (from c in league.UserLeagueCategories where c.CategoryId == cat.Id select c).FirstOrDefault();
                                    if (ulc != null)
                                    {
                                        ulc.PointsPerStat = Convert.ToDouble(statNode["value"].InnerText);
                                    }
                                    else
                                    {
                                        if (logger != null)
                                            logger.LogError("No match for category " + statId);
                                    }
                                }

                            }
                        }

                    }
                }

                league.FillUserLeagueCategoriesCode(categories);
            }
            catch (Exception ex)
            {

            }

            return league;
        }


        public Draft ImportDraft(
            UserAuth userAuth,
            Season season,
            string providerLeagueId,
            List<FantasyProviderPlayer> fantasyProviderPlayers,
            ILogger logger
            )
        {
            var userLeague = new UserLeague();
            userLeague.SeasonId = season.Id;
            userLeague.ProviderLeagueId = providerLeagueId;

            return ImportDraft(userAuth, userLeague, fantasyProviderPlayers, season.YahooId, logger);
        }

        public Draft ImportDraft(
            UserAuth userAuth,
            UserLeague userLeague,
            List<FantasyProviderPlayer> fantasyProviderPlayers,
            string yahooSeasonId,
            ILogger logger
            )
        {
            Draft draft = null;

            string leagueKey = yahooSeasonId + ".l." + userLeague.ProviderLeagueId;
            string url = "https://fantasysports.yahooapis.com/fantasy/v2/league/" + leagueKey + "/draftresults";

            string data = GetYahooAPIXML(userAuth, url);

            FileLib filelib = new FileLib();
            filelib.WriteData(config, "ydraft", userAuth.UserId, userLeague.ProviderLeagueId, "xml", data);

            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(data);

                if (xml["fantasy_content"] != null)
                {
                    draft = new Draft();
                    draft.DraftPlayers = new List<DraftPlayer>();
                    draft.ProviderLeagueId = userLeague.ProviderLeagueId;
                    draft.DraftDate = userLeague.DraftDate.GetValueOrDefault();
                    draft.IsMoney = userLeague.IsMoney;
                    draft.FantasyProviderId = 1;
                    draft.IsAuction = userLeague.IsAuction;
                    draft.SeasonId = userLeague.SeasonId;
                    draft.ImportUserLeague(userLeague);

                    XmlNode leagueNode = xml["fantasy_content"]["league"];
                    draft.IsMock = (leagueNode["is_mock"] != null && leagueNode["is_mock"].InnerText == "1");
                    draft.IsFinished = (leagueNode["draft_status"].InnerText == "postdraft");
                    draft.IsPreDraft = (leagueNode["draft_status"].InnerText == "predraft");
                    draft.IsLive = (leagueNode["draft_status"].InnerText == "draft");
                    draft.Title = userLeague.Title;
                    draft.NumberOfTeams = userLeague.NumberOfTeams;
                    draft.LeagueSize = userLeague.NumberOfTeams * userLeague.PlayersPerTeam;
                    draft.Title = leagueNode["name"].InnerText;
                    draft.IsProLeague = (leagueNode["is_pro_league"].InnerText == "1");
                    draft.LeagueType = (leagueNode["scoring_type"].InnerText == "head") ? "H" : "R";

                    XmlNode resultsNode = leagueNode["draft_results"];
                    foreach (XmlNode playerNode in resultsNode.ChildNodes)
                    {
                        int pick = 0;
                        if (playerNode["pick"] != null)
                        {
                            pick = Convert.ToInt32(playerNode["pick"].InnerText);
                            if (playerNode["team_key"] != null)
                            {
                                var teamKey = playerNode["team_key"].InnerText;
                                teamKey = teamKey.Replace(leagueKey + ".t.", "");
                                if (draft.DraftUserLeagueTeams.Find(t => t.ProviderId == teamKey) == null)
                                {
                                    UserLeagueTeam draftTeam = new UserLeagueTeam();
                                    draftTeam.ProviderId = teamKey;
                                    draftTeam.ProviderId = draftTeam.ProviderId.Replace(leagueKey + ".t.", "");
                                    draftTeam.DraftOrder = pick;
                                    draft.DraftUserLeagueTeams.Add(draftTeam);
                                }
                            }
                        }

                        if (playerNode["player_key"] == null)
                            continue;

                        string playerKey = playerNode["player_key"].InnerText;
                        playerKey = playerKey.Replace(yahooSeasonId + ".p.", "");
                        var providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == 1 && pp.ProviderId == playerKey select pp).FirstOrDefault();
                        if (providerPlayer != null)
                        {
                            DraftPlayer draftPlayer = new DraftPlayer();
                            if (pick > 0)
                                draftPlayer.DraftOrder = pick;
                            if (playerNode["cost"] != null)
                                draftPlayer.Price = Convert.ToInt32(playerNode["cost"].InnerText);
                            draftPlayer.ProviderTeamId = playerNode["team_key"].InnerText;
                            draftPlayer.ProviderTeamId = draftPlayer.ProviderTeamId.Replace(leagueKey + ".t.", "");
                            draftPlayer.PlayerId = providerPlayer.Player.Id;
                            draft.DraftPlayers.Add(draftPlayer);
                        }
                        else
                        {
                            if (logger != null)
                                logger.LogError("Missing Yahoo draft player " + playerKey + " for league " + userLeague.ProviderLeagueId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return draft;
        }

        public List<UserInvitation> GetUserInvitations()
        {
            return db.UserInvitations.ToList();
        }

        public UserInvitation UpdateUserInvitation(UserInvitation userInvitation)
        {
            var updateUserInvitation = (from u in db.UserInvitations where u.Id == userInvitation.Id select u).FirstOrDefault();
            if (updateUserInvitation != null)
            {
                updateUserInvitation.DateUsed = DateTime.UtcNow;
                db.SaveChanges();
            }

            return updateUserInvitation;
        }
    }
}
