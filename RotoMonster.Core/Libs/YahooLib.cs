using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RotoMonster.Core.Libs
{
    public class YahooLib
    {
        private readonly IConfiguration config;
        private readonly string yahooSeasonId;
        private readonly ILogger logger;

        public string ConsumerKey { get; set; } = "";
        public string ConsumerSecret { get; set; } = "";
        private FileLib filelib = new FileLib();

        public YahooLib(IConfiguration config, string key, string secret, string yahooSeasonId, ILogger logger)
        {
            this.config = config;
            ConsumerKey = key;
            ConsumerSecret = secret;
            this.yahooSeasonId = yahooSeasonId;
            this.logger = logger;
        }

        // basic flow
        // - register an app with Yahoo API which gives you the ConsumerKey and ConsumerSecret
        // - call GetAuthorizationURL to get a URL you can send the user to to get their permission to access their account
        // - pass this call a URL to your website to process the call from Yahoo (domain likely needs to match one registered with Yahoo)
        // - once the user confirms, Yahoo will call your URL with a query string with a "code" parameter
        // - use the code to call GetAccessToken to get an access token and a refresh token
        // - use the access token to make calls to the API by calling GetYahooAPIXML
        // - access tokens only last one hour so you need to call RefreshAccessToken with the refresh token to get new access and refresh tokens
        // - i store the date/time the token was generated and if more than 1 hour, i call RefreshAccessToken before making GetYahooAPIXML call
        // end basic flow

        // have user go to this URL
        // once confirmed by user, Yahoo calls the callbackURL with query paramerter "code"
        // use this for GetAccessToken
        public string GetAuthorizationURL(string callbackURL = "")
        {
            string url = "https://api.login.yahoo.com/oauth2/request_auth?client_id=" + ConsumerKey + "&response_type=code&language=en-us&redirect_uri=";
            if (callbackURL.Length > 0)
                url += callbackURL.Replace("/", "%2F");
            else
                url += "oob";   // no url so user will need to copy Yahoo code

            return url;
        }

        // use code generated using the above process to get accessToken and refreshToken
        public bool GetAccessToken(string code, ref string outAccessToken, ref string outRefreshToken)
        {
            Uri address = new Uri("https://api.login.yahoo.com/oauth2/get_token");
            HttpWebRequest req = WebRequest.Create(address) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            StringBuilder data = new StringBuilder();
            data.Append("&client_id=" + ConsumerKey);
            data.Append("&client_secret=" + ConsumerSecret);
            data.Append("&redirect_uri=oob");
            data.Append("&code=" + code);
            data.Append("&grant_type=authorization_code");
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());
            req.ContentLength = byteData.Length;

            using (Stream postStream = req.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            try
            {
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    string r = reader.ReadToEnd();

                    JObject rss = JObject.Parse(r);
                    outAccessToken = System.Convert.ToString(rss["access_token"]);
                    outRefreshToken = System.Convert.ToString(rss["refresh_token"]);

                    return true;
                }
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        // call to get a new access and refresh tokens
        // for yahoo, access tokens only last 1 hour
        // not sure if refresh tokens expire so storing new ones just in case
        public bool RefreshAccessToken(string refreshToken, ref string accessToken, ref string outRefreshToken)
        {
            Uri address = new Uri("https://api.login.yahoo.com/oauth2/get_token");
            HttpWebRequest req = WebRequest.Create(address) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            StringBuilder data = new StringBuilder();
            data.Append("&client_id=" + ConsumerKey);
            data.Append("&client_secret=" + ConsumerSecret);
            data.Append("&redirect_uri=oob");
            data.Append("&refresh_token=" + refreshToken);
            data.Append("&grant_type=refresh_token");
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());
            req.ContentLength = byteData.Length;

            using (Stream postStream = req.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            try
            {
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    string r = reader.ReadToEnd();

                    JObject rss = JObject.Parse(r);
                    accessToken = System.Convert.ToString(rss["access_token"]);
                    outRefreshToken = System.Convert.ToString(rss["refresh_token"]);

                    return true;
                }
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        // make a call to the Yahoo API, returning XML
        // NOTE: YQL no longer supported by Yahoo!
        public string GetYahooAPIXML(string url, string accessToken)
        {
            string data = "";
            try
            {
                using (var web = new WebClient())
                {
                    web.Headers.Add("Authorization", "Bearer " + accessToken);
                    data = web.DownloadString(url);
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return data;
        }

        public bool IsConnected(UserAuth userAuth)
        {
            if (userAuth != null && userAuth.YahooAccessToken != null && userAuth.YahooRefreshToken != null)
            {
                return userAuth.YahooAccessToken.Length > 0 && userAuth.YahooRefreshToken.Length > 0;
            }

            return false;
        }

        //public string GetLeaguesXml1(UserAuth userAuth)
        //{
        //    string data = GetYahooAPIXML("https://fantasysports.yahooapis.com/fantasy/v2/users;use_login=1/games;game_keys=" + yahooSeasonId + "/leagues", userAuth.YahooAccessToken);

        //    return data;
        //}

        //public List<UserLeague> GetLeagues1(string leaguesXml)
        //{
        //    List<UserLeague> userLeagues = new List<UserLeague>();

        //    XmlDocument xml = new XmlDocument();
        //    xml.LoadXml(leaguesXml);

        //    if (xml["fantasy_content"] != null)
        //    {
        //        foreach (XmlNode userNode in xml["fantasy_content"]["users"].ChildNodes)
        //        {
        //            foreach (XmlNode gameNode in userNode["games"].ChildNodes)
        //            {
        //                foreach (XmlNode leagueNode in gameNode["leagues"].ChildNodes)
        //                {
        //                    UserLeague league = new UserLeague();
        //                    league.ProviderLeagueId = leagueNode["league_id"].InnerText;
        //                    league.Title = leagueNode["name"].InnerText;
        //                    league.NumberOfTeams = Convert.ToInt32(leagueNode["num_teams"].InnerText);
        //                    if (leagueNode["scoring_type"].InnerText == "head")
        //                        league.LeagueType = "H";
        //                    else if (leagueNode["scoring_type"].InnerText == "roto")
        //                        league.LeagueType = "R";
        //                    userLeagues.Add(league);
        //                }
        //            }
        //        }
        //    }

        //    return userLeagues;
        //}

        //public Draft ImportDraft1(
        //    UserAuth userAuth,
        //    Season season,
        //    string providerLeagueId,
        //    List<FantasyProviderPlayer> fantasyProviderPlayers
        //    )
        //{
        //    var userLeague = new UserLeague();
        //    userLeague.SeasonId = season.Id;
        //    userLeague.ProviderLeagueId = providerLeagueId;

        //    return ImportDraft1(userAuth, userLeague, fantasyProviderPlayers);
        //}

        //public Draft ImportDraft1(
        //    UserAuth userAuth,
        //    UserLeague userLeague,
        //    List<FantasyProviderPlayer> fantasyProviderPlayers
        //    )
        //{
        //    Draft draft = null;

        //    if (!IsConnected(userAuth))
        //        return draft;

        //    string leagueKey = yahooSeasonId + ".l." + userLeague.ProviderLeagueId;
        //    string url = "https://fantasysports.yahooapis.com/fantasy/v2/league/" + leagueKey + "/draftresults";

        //    string data = GetYahooAPIXML(url, userAuth.YahooAccessToken);

        //    filelib.WriteData(config, "ydraft", userAuth.UserId, userLeague.ProviderLeagueId, "xml", data);

        //    try
        //    {
        //        XmlDocument xml = new XmlDocument();
        //        xml.LoadXml(data);

        //        if (xml["fantasy_content"] != null)
        //        {
        //            draft = new Draft();
        //            draft.DraftPlayers = new List<DraftPlayer>();
        //            draft.ProviderLeagueId = userLeague.ProviderLeagueId;
        //            draft.DraftDate = userLeague.DraftDate.GetValueOrDefault();
        //            draft.IsMoney = userLeague.IsMoney;
        //            draft.FantasyProviderId = 1;
        //            draft.IsAuction = userLeague.IsAuction;
        //            draft.SeasonId = userLeague.SeasonId;
        //            draft.ImportUserLeague(userLeague);

        //            XmlNode leagueNode = xml["fantasy_content"]["league"];
        //            draft.IsMock = (leagueNode["is_mock"] != null && leagueNode["is_mock"].InnerText == "1");
        //            draft.IsFinished = (leagueNode["draft_status"].InnerText == "postdraft");
        //            draft.IsPreDraft = (leagueNode["draft_status"].InnerText == "predraft");
        //            draft.IsLive = (leagueNode["draft_status"].InnerText == "draft");
        //            draft.Title = userLeague.Title;
        //            draft.NumberOfTeams = Convert.ToInt32(leagueNode["num_teams"].InnerText);
        //            draft.LeagueSize = userLeague.NumberOfTeams * userLeague.PlayersPerTeam;
        //            draft.Title = leagueNode["name"].InnerText;
        //            draft.IsProLeague = (leagueNode["is_pro_league"].InnerText == "1");
        //            draft.LeagueType = (leagueNode["scoring_type"].InnerText == "head") ? "H" : "R";

        //            XmlNode resultsNode = leagueNode["draft_results"];
        //            foreach (XmlNode playerNode in resultsNode.ChildNodes)
        //            {
        //                int pick = 0;
        //                if (playerNode["pick"] != null)
        //                {
        //                    pick = Convert.ToInt32(playerNode["pick"].InnerText);
        //                    if (playerNode["team_key"] != null)
        //                    {
        //                        var teamKey = playerNode["team_key"].InnerText;
        //                        teamKey = teamKey.Replace(leagueKey + ".t.", "");
        //                        if (draft.DraftUserLeagueTeams.Find(t => t.ProviderId == teamKey) == null)
        //                        {
        //                            UserLeagueTeam draftTeam = new UserLeagueTeam();
        //                            draftTeam.ProviderId = teamKey;
        //                            draftTeam.ProviderId = draftTeam.ProviderId.Replace(leagueKey + ".t.", "");
        //                            draftTeam.DraftOrder = pick;
        //                            draft.DraftUserLeagueTeams.Add(draftTeam);
        //                        }
        //                    }
        //                }

        //                draft.LeagueSize++;

        //                if (playerNode["player_key"] == null)
        //                    continue;

        //                string playerKey = playerNode["player_key"].InnerText;
        //                playerKey = playerKey.Replace(yahooSeasonId + ".p.", "");
        //                var providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == 1 && pp.ProviderId == playerKey select pp).FirstOrDefault();
        //                if (providerPlayer != null)
        //                {
        //                    DraftPlayer draftPlayer = new DraftPlayer();
        //                    if (pick > 0)
        //                        draftPlayer.DraftOrder = pick;
        //                    if (playerNode["cost"] != null)
        //                        draftPlayer.Price = Convert.ToInt32(playerNode["cost"].InnerText);
        //                    draftPlayer.ProviderTeamId = playerNode["team_key"].InnerText;
        //                    draftPlayer.ProviderTeamId = draftPlayer.ProviderTeamId.Replace(leagueKey + ".t.", "");
        //                    draftPlayer.PlayerId = providerPlayer.Player.Id;
        //                    draft.DraftPlayers.Add(draftPlayer);
        //                }
        //                else
        //                {
        //                    if (logger != null)
        //                        logger.LogError("Missing Yahoo player " + playerKey);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return draft;
        //}

        //public UserLeague ImportUserLeague1(
        //    UserAuth userAuth,
        //    Season season,
        //    string yahooLeagueId,
        //    List<ActiveRosterSpot> activeRosterSpots,
        //    List<Category> categories)
        //{
        //    UserLeague league = null;

        //    string url = "https://fantasysports.yahooapis.com/fantasy/v2/league/" + yahooSeasonId + ".l." + yahooLeagueId + "/settings";

        //    string data = GetYahooAPIXML(url, userAuth.YahooAccessToken);

        //    filelib.WriteData(config, "yleague", userAuth.UserId, yahooLeagueId, "xml", data);

        //    try
        //    {
        //        XmlDocument xml = new XmlDocument();
        //        xml.LoadXml(data);

        //        if (xml["fantasy_content"] != null)
        //        {
        //            league = new UserLeague();
        //            league.UserId = userAuth.UserId;
        //            league.SeasonId = season.Id;
        //            league.TrackLeague = true;
        //            league.ProviderLeagueId = yahooLeagueId;
        //            league.FantasyProviderId = 1;

        //            XmlNode leagueNode = xml["fantasy_content"]["league"];

        //            league.HasDrafted = (leagueNode["draft_status"].InnerText == "postdraft");

        //            XmlNode sNode = leagueNode["settings"];
        //            if (leagueNode["settings"]["draft_time"] != null)
        //            {
        //                long epoch = Convert.ToInt64(leagueNode["settings"]["draft_time"].InnerText);
        //                DateTime draftDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);
        //                draftDate = draftDate.AddSeconds(epoch);
        //                league.DraftDate = draftDate;
        //            }
        //            league.Title = leagueNode["name"].InnerText;
        //            league.DisplayTitle = league.Title;
        //            league.IsMoney = (leagueNode["is_cash_league"].InnerText == "1");
        //            league.IsProLeague = (leagueNode["is_pro_league"].InnerText == "1");
        //            league.IsAuction = (sNode["is_auction_draft"].InnerText == "1");
        //            league.NumberOfTeams = Convert.ToInt32(leagueNode["num_teams"].InnerText);
        //            league.LeagueType = (leagueNode["scoring_type"].InnerText == "head") ? "H" : "R";
        //            league.ContinuousWaivers = (sNode["waiver_rule"] != null && sNode["waiver_rule"].InnerText == "continuous");
        //            league.WaiverType = (sNode["waiver_type"] != null ? sNode["waiver_type"].InnerText : "");
        //            league.WaiverRule = (sNode["waiver_rule"] != null ? sNode["waiver_rule"].InnerText : "");
        //            if (leagueNode["weekly_deadline"].InnerText == "1")
        //            {
        //                league.LineupFrequency = "W";
        //                league.SameDayTransactions = true;
        //            }
        //            else
        //            {
        //                league.LineupFrequency = "D";
        //                league.SameDayTransactions = (leagueNode["weekly_deadline"].InnerText == "intraday");
        //            }
        //            if (leagueNode["scoring_type"] != null && leagueNode["scoring_type"].InnerText == "point")
        //                league.ScoringSystem = "P";
        //            else
        //                league.ScoringSystem = "C";

        //            if (sNode["uses_playoff"].InnerText == "1")
        //            {

        //            }

        //            if (sNode["roster_positions"] != null)
        //            {
        //                foreach (XmlNode posNode in sNode["roster_positions"].ChildNodes)
        //                {
        //                    string posText = posNode["position"].InnerText;
        //                    if (posText == "IL" || posText == "IR" || posText == "DL")
        //                    {
        //                        league.IRSpots = Convert.ToInt32(posNode["count"].InnerText);
        //                        continue;
        //                    }

        //                    if (posText == "BN")
        //                    {
        //                        league.PlayersPerTeam += Convert.ToInt32(posNode["count"].InnerText);
        //                    }
        //                    else
        //                    {
        //                        UserLeagueActiveRosterSpot rs = new UserLeagueActiveRosterSpot();
        //                        rs.NumberOfPlayers = Convert.ToInt32(posNode["count"].InnerText);
        //                        league.PlayersPerTeam += rs.NumberOfPlayers;
        //                        var ars = (from a in activeRosterSpots where a.Title == posText || a.YahooTitle == posText select a).FirstOrDefault();
        //                        if (ars != null)
        //                        {
        //                            rs.ActiveRosterSpotId = ars.Id;
        //                            league.UserLeagueActiveRosterSpots.Add(rs);
        //                        }
        //                        else
        //                        {
        //                            if (logger != null)
        //                                logger.LogError("No match for active roster spot " + posText);
        //                        }

        //                    }
        //                }

        //                if (sNode["stat_categories"] != null && sNode["stat_categories"]["stats"] != null)
        //                {
        //                    foreach (XmlNode statNode in sNode["stat_categories"]["stats"].ChildNodes)
        //                    {
        //                        UserLeagueCategory ulc = new UserLeagueCategory();
        //                        ulc.IsActive = true;
        //                        if (statNode["is_only_display_stat"] != null && statNode["is_only_display_stat"].InnerText == "1")
        //                            continue;
        //                        string statId = statNode["stat_id"].InnerText;
        //                        string statName = statNode["name"].InnerText;
        //                        string positionType = statNode["position_type"].InnerText;
        //                        if (positionType == "DP")
        //                            continue;

        //                        var cat = (from c in categories where c.YahooId == statId select c).FirstOrDefault();
        //                        if (cat != null)
        //                        {
        //                            ulc.CategoryId = cat.Id;
        //                            league.UserLeagueCategories.Add(ulc);
        //                        }
        //                        else
        //                        {
        //                            if (logger != null)
        //                                logger.LogError("No match for category " + statId);
        //                        }
        //                    }
        //                }

        //                if (sNode["stat_modifiers"] != null && sNode["stat_modifiers"]["stats"] != null)
        //                {
        //                    league.ScoringSystem = "P";
        //                    foreach (XmlNode statNode in sNode["stat_modifiers"]["stats"].ChildNodes)
        //                    {
        //                        string statId = statNode["stat_id"].InnerText;
        //                        var cat = (from c in categories where c.YahooId == statId select c).FirstOrDefault();
        //                        if (cat != null)
        //                        {
        //                            var ulc = (from c in league.UserLeagueCategories where c.CategoryId == cat.Id select c).FirstOrDefault();
        //                            if (ulc != null)
        //                            {
        //                                ulc.PointsPerStat = Convert.ToDouble(statNode["value"].InnerText);
        //                            }
        //                            else
        //                            {
        //                                if (logger != null)
        //                                    logger.LogError("No match for category " + statId);
        //                            }
        //                        }

        //                    }
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    league.FillUserLeagueCategoriesCode(categories);

        //    return league;
        //}

        //public List<UserLeagueTeam> GetUserLeagueTeams1(
        //    UserAuth userAuth,
        //    UserLeague userLeague,
        //    List<FantasyProviderPlayer> fantasyProviderPlayers,
        //    List<UserLeagueMissingPlayer> userLeagueMissingPlayers,
        //    XmlDocument inXml = null)
        //{
        //    List<UserLeagueTeam> teams = new List<UserLeagueTeam>();

        //    string data = "";
        //    if (inXml == null)
        //    {
        //        DateTime rosterDate = DateTime.Today.AddDays(7);
        //        string url = "https://fantasysports.yahooapis.com/fantasy/v2/league/" + yahooSeasonId + ".l." + userLeague.ProviderLeagueId + "/teams/roster/players";
        //        url += String.Format(";date={0}-{1:00}-{2:00}", rosterDate.Year, rosterDate.Month, rosterDate.Day);

        //        data = GetYahooAPIXML(url, userAuth.YahooAccessToken);

        //        filelib.WriteData(config, "yteams", userAuth.UserId, userLeague.ProviderLeagueId, "xml", data);
        //    }

        //    try
        //    {
        //        XmlDocument xml = inXml;
        //        if (inXml == null)
        //        {
        //            xml = new XmlDocument();
        //            xml.LoadXml(data);
        //        }
        //        if (xml["fantasy_content"] != null)
        //        {
        //            foreach (XmlNode teamNode in xml["fantasy_content"]["league"]["teams"].ChildNodes)
        //            {
        //                string teamKey = teamNode["team_id"].InnerText;
        //                UserLeagueTeam team = team = new UserLeagueTeam();
        //                team.UserLeagueId = userLeague.Id;
        //                team.ProviderId = teamKey;
        //                team.Title = teamNode["name"].InnerText;
        //                XmlNode managersNode = teamNode["manager"];
        //                team.DraftOrder = Convert.ToInt32(teamKey);
        //                teams.Add(team);

        //                foreach (XmlNode managerNode in teamNode["managers"].ChildNodes)
        //                {
        //                    if (managerNode["is_current_login"] != null && managerNode["is_current_login"].InnerText == "1")
        //                    {
        //                        userLeague.MyProviderTeamId = team.ProviderId;
        //                        userLeague.MyTeamTitle = team.Title;
        //                    }
        //                }

        //                foreach (XmlNode playerNode in teamNode["roster"]["players"].ChildNodes)
        //                {
        //                    string yahooId = playerNode["player_id"].InnerText;
        //                    var providerPlayer = (from pp in fantasyProviderPlayers where pp.FantasyProvider.Id == 1 && pp.ProviderId == yahooId select pp).FirstOrDefault();
        //                    if (providerPlayer != null)
        //                    {
        //                        UserLeagueTeamPlayer tp = new UserLeagueTeamPlayer();
        //                        tp.PlayerId = providerPlayer.Player.Id;
        //                        string rosterSpot = playerNode["selected_position"]["position"].InnerText;
        //                        tp.IsIR = (rosterSpot == "IL" || rosterSpot == "IR");
        //                        tp.IsActive = !tp.IsIR && rosterSpot != "BN";
        //                        team.UserLeagueTeamPlayers.Add(tp);
        //                    }
        //                    else
        //                    {
        //                        if (userLeagueMissingPlayers != null)
        //                        {
        //                            var missingPlayer = new UserLeagueMissingPlayer();
        //                            missingPlayer.ProviderId = yahooId;
        //                            userLeagueMissingPlayers.Add(missingPlayer);
        //                        }

        //                        if (logger != null)
        //                            logger.LogError("Missing Yahoo player " + yahooId);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return teams;
        //}

        public List<UserLeagueTeam> SortDraftOrderTeams(List<UserLeagueTeam> userLeagueTeams)
        {
            return (from t in userLeagueTeams orderby t.DraftOrder ascending select t).ToList();
        }


        public string GetYahooGameKeysXml(UserAuth userAuth, string sport)
        {
            string data = GetYahooAPIXML("https://fantasysports.yahooapis.com/fantasy/v2/games;game_codes=" + sport.ToLower(), userAuth.YahooAccessToken);

            filelib.WriteData(config, "ykeys", userAuth.UserId, "", "xml", data);

            return data;
        }

    }

}
