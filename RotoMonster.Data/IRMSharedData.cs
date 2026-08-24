using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace RotoMonster.Data
{
    public interface IRMSharedData
    {
        List<ApplicationUser> GetUsers();
        UserAuth GetUserAuth(string userId);
        List<UserAuth> UserAuths { get; }
        UserAuth AddYahooUserAuth(string userId, string accessToken, string refreshToken);
        UserAuth AddESPNUserAuth(string userId, string swid, string s2);
        UserAuth AddFanTraxUserAuth(string userId, string fanTraxEmail);
        UserAuth AddSleeperUserAuth(string userId, string sleeperName, string sleeperId);
        UserAuth AddUserAuth(UserAuth userAuth);
        void ClearYahooAuth(string userId);
        void ClearESPNAuth(string userId);
        void ClearFanTraxAuth(string userId);
        void ClearSleeperAuth(string userId);

        // Yahoo Calls
        string GetYahooAPIXML(UserAuth userAuth, string url);

        string GetLeaguesXml(UserAuth userAuth, string yahooSeasonId);

        List<UserLeague> GetLeagues(string leaguesXml);
        List<UserInvitation> GetUserInvitations();
        UserInvitation UpdateUserInvitation(UserInvitation userInvitation);

        UserLeague ImportUserLeague(
            UserAuth userAuth,
            Season season,
            string yahooLeagueId,
            List<ActiveRosterSpot> activeRosterSpots,
            List<Category> categories,
            ILogger logger);

        Draft ImportDraft(
            UserAuth userAuth,
            Season season,
            string providerLeagueId,
            List<FantasyProviderPlayer> fantasyProviderPlayers,
            ILogger logger
            );

        Draft ImportDraft(
            UserAuth userAuth,
            UserLeague userLeague,
            List<FantasyProviderPlayer> fantasyProviderPlayers,
            string yahooSeasonId,
            ILogger logger
            );

        List<UserLeagueTeam> GetUserLeagueTeams(
            UserAuth userAuth,
            string yahooSeasonId,
            UserLeague userLeague,
            List<FantasyProviderPlayer> fantasyProviderPlayers,
            List<UserLeagueMissingPlayer> userLeagueMissingPlayers,
            ILogger logger,
            XmlDocument inXml = null);


    }
}
