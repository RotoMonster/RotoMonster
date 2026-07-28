using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Identity.UI;

namespace RotoMonster.Core.Libs
{
    public class UserLeagueLib
    {
        public SelectList ScoringSystemList
        {
            get
            {
                var items = new List<SelectListItem>();
                items.Add(new SelectListItem("Categories", "C"));
                items.Add(new SelectListItem("Points Per Stat", "P"));
                return new SelectList(items, "Value", "Text");
            }
        }

        public SelectList LeagueTypeList
        {
            get
            {
                var items = new List<SelectListItem>();
                items.Add(new SelectListItem("H2H", "H"));
                items.Add(new SelectListItem("Roto", "R"));
                return new SelectList(items, "Value", "Text");
            }
        }

        public SelectList LineupFrequencyList
        {
            get
            {
                var items = new List<SelectListItem>();
                items.Add(new SelectListItem("Daily", "D"));
                items.Add(new SelectListItem("Weekly", "W"));
                return new SelectList(items, "Value", "Text");
            }
        }

        public SelectList CategorySelectionList
        {
            get
            {
                var items = new List<SelectListItem>();
                items.Add(new SelectListItem("Off", "Off"));
                items.Add(new SelectListItem("On", "On"));
                items.Add(new SelectListItem("Punt", "Punt"));
                return new SelectList(items, "Value", "Text");
            }
        }

        public List<UserLeagueTeamPlayer> RemoveOutUserLeagueTeamPlayers(
            List<UserLeagueTeamPlayer> userLeagueTeamPlayers,
            List<PlayerStatus> playerStatuses
            )
        {
            var outTeamPlayers = new List<UserLeagueTeamPlayer>();
            foreach (var tp in userLeagueTeamPlayers)
            {
                var playerStatus = (from ps in playerStatuses where ps.PlayerId == tp.PlayerId select ps).FirstOrDefault();
                if (playerStatus != null && playerStatus.IsOut)
                    continue;
                outTeamPlayers.Add(tp);
            }

            return outTeamPlayers;
        }


        public List<UserLeagueTeamPlayer> RemoveNoGameUserLeagueTeamPlayers(
            List<SeasonPlayer> seasonPlayers,
            List<UserLeagueTeamPlayer> userLeagueTeamPlayers,
            List<Game> games
            )
        {
            var outTeamPlayers = new List<UserLeagueTeamPlayer>();
            foreach (var tp in userLeagueTeamPlayers)
            {
                var seasonPlayer = (from sp in seasonPlayers where sp.PlayerId == tp.PlayerId select sp).FirstOrDefault();
                if (seasonPlayer != null)
                {
                    var game = (from g in games where g.IncludesTeam(seasonPlayer.TeamId) select g).FirstOrDefault();
                    if (game == null)
                        continue;
                }
                outTeamPlayers.Add(tp);
            }

            return outTeamPlayers;
        }
    }
}
