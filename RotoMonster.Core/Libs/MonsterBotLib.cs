using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RotoMonster.Core.Libs
{
    public class MonsterBotLib
    {

        public List<MonsterBotPlayer> GetMonsterBotPlayers(
            List<UserLeagueActiveRosterSpot> userLeagueActiveRosterSpots,
            List<UserLeagueTeamPlayer> userLeagueTeamPlayers,
            PlayerType playerType,
            List<OwnershipPlayer> ownershipPlayers,
            Season season,
            List<SeasonPlayer> seasonPlayers,
            List<PlayerStatus> playerStatuses,
            List<PositionSourcePlayer> positionSourcePlayers,
            List<PlayerGameState> playerGameStates,
            List<Game> games
            )
        {
            var userLeagueLib = new UserLeagueLib();

            List<MonsterBotPlayer> monsterBotPlayers = new List<MonsterBotPlayer>();
            List<DisplayPlayer> optPlayers = null;
            if (userLeagueActiveRosterSpots != null)
            {
                if (userLeagueTeamPlayers.Count > 0)
                {
                    var teamPlayers = userLeagueLib.RemoveOutUserLeagueTeamPlayers(userLeagueTeamPlayers, playerStatuses);
                    teamPlayers = userLeagueLib.RemoveNoGameUserLeagueTeamPlayers(seasonPlayers, teamPlayers, games);
                    optPlayers = GetOptimumActivePlayers(userLeagueActiveRosterSpots, teamPlayers, playerType, ownershipPlayers, season, seasonPlayers, positionSourcePlayers, playerGameStates, playerStatuses, games, false);
                    foreach (var tp in userLeagueTeamPlayers)
                    {
                        var seasonPlayer = (from sp in seasonPlayers where sp.PlayerId == tp.PlayerId && sp.PlayerTypeId == playerType.Id select sp).FirstOrDefault();
                        if (seasonPlayer == null)
                            continue;

                        var game = (from g in games where g.HomeTeam.Id == seasonPlayer.TeamId || g.AwayTeam.Id == seasonPlayer.TeamId select g).FirstOrDefault();
                        if (game != null && game.HasStarted)
                            continue;

                        var monsterBotPlayer = new MonsterBotPlayer();
                        monsterBotPlayer.DisplayPlayer.SeasonPlayer = seasonPlayer;
                        monsterBotPlayer.DisplayPlayer.Positions = (from p1 in positionSourcePlayers where p1.PlayerId == seasonPlayer.Player.Id select p1.Position).ToList();
                        monsterBotPlayer.DisplayPlayer.Game = game;
                        var optPlayer = (from op in optPlayers where op.SeasonPlayer.PlayerId == tp.PlayerId select op).FirstOrDefault();
                        var playerStatus = (from ps in playerStatuses where ps.PlayerId == tp.PlayerId select ps).FirstOrDefault();

                        if (tp.IsActive)
                        {
                            if (playerStatus != null && (playerStatus.PlayerStatusType.UsesDate.GetValueOrDefault() || playerStatus.PlayerStatusType.PlayType == "O"))
                            {
                                //var monsterBotPlayerComment = new MonsterBotPlayerComment();
                                //monsterBotPlayerComment.Icon = RotoMonsterIcons.MonsterBotInjured;
                                //monsterBotPlayerComment.IsAlert = true;
                                //monsterBotPlayerComment.Text = "He is " + playerStatus.PlayerStatusType.Title.ToUpper() + " so MonsterBot recommends you BENCH him.";
                                //monsterBotPlayer.MonsterBotPlayerComments.Add(monsterBotPlayerComment);
                            }
                            else if (playerStatus != null && playerStatus.PlayerStatusType.IsUndetermined.GetValueOrDefault())
                            {
                                //var monsterBotPlayerComment = new MonsterBotPlayerComment();
                                //monsterBotPlayerComment.Icon = RotoMonsterIcons.MonsterBotQuestionable;
                                //monsterBotPlayerComment.IsWarning = true;
                                //monsterBotPlayerComment.Text = "He is currently " + playerStatus.PlayerStatusType.Title.ToUpper() + " so please check with MonsterBot later.";
                                //monsterBotPlayer.MonsterBotPlayerComments.Add(monsterBotPlayerComment);
                            }
                            else if (game == null)
                            {
                                //var monsterBotPlayerComment = new MonsterBotPlayerComment();
                                //monsterBotPlayerComment.Icon = RotoMonsterIcons.MonsterBotBench;
                                //monsterBotPlayerComment.IsAlert = true;
                                //monsterBotPlayerComment.Text = "He is OFF so MonsterBot recommends you BENCH him.";
                                //monsterBotPlayer.MonsterBotPlayerComments.Add(monsterBotPlayerComment);
                            }

                            if (optPlayer != null)
                            {
                                var monsterBotPlayerComment = new MonsterBotPlayerComment();
                                monsterBotPlayerComment.Icon = RotoMonsterIcons.MonsterBotActiveApproved;
                                monsterBotPlayerComment.IsOK = true;
                                monsterBotPlayerComment.Text = "MonsterBot recommends you keep him ACTIVE.";
                                monsterBotPlayer.MonsterBotPlayerComments.Add(monsterBotPlayerComment);
                            }
                            else
                            {
                                //var monsterBotPlayerComment = new MonsterBotPlayerComment();
                                //monsterBotPlayerComment.Icon = RotoMonsterIcons.MonsterBotBench;
                                //monsterBotPlayerComment.IsWarning = true;
                                //monsterBotPlayerComment.Text = "MonsterBot recommends you BENCH him.";
                                //monsterBotPlayer.MonsterBotPlayerComments.Add(monsterBotPlayerComment);
                            }

                        }

                        else if (!tp.IsActive)
                        {
                            if (optPlayer != null)
                            {
                                var monsterBotPlayerComment = new MonsterBotPlayerComment();
                                monsterBotPlayerComment.Icon = RotoMonsterIcons.MonsterBotActivate;
                                monsterBotPlayerComment.IsWarning = true;
                                monsterBotPlayerComment.Text = "MonsterBot recommends you ACTIVATE him.";
                                monsterBotPlayer.MonsterBotPlayerComments.Add(monsterBotPlayerComment);
                            }
                        }

                        if (monsterBotPlayer.MonsterBotPlayerComments.Count > 0)
                        {
                            monsterBotPlayers.Add(monsterBotPlayer);
                        }
                    }
                }

            }

            return monsterBotPlayers;
        }

        public List<MonsterBotPlayer> GetAllMonsterBotPlayers(
            List<PlayerType> playerTypes,
            List<UserLeagueActiveRosterSpot> userLeagueActiveRosterSpots,
            List<UserLeagueTeamPlayer> userLeagueTeamPlayers,
            List<OwnershipPlayer> ownershipPlayers,
            Season season,
            List<SeasonPlayer> seasonPlayers,
            List<PlayerStatus> playerStatuses,
            List<PositionSourcePlayer> positionSourcePlayers,
            List<PlayerGameState> playerGameStates,
            List<Game> games)
        {
            var monsterBotPlayers = new List<MonsterBotPlayer>();

            foreach (var pt in playerTypes)
            {
                var ptmbp = GetMonsterBotPlayers(userLeagueActiveRosterSpots, userLeagueTeamPlayers, pt, ownershipPlayers, season, seasonPlayers, playerStatuses, positionSourcePlayers, playerGameStates, games);
                foreach (var mbp in ptmbp)
                    monsterBotPlayers.Add(mbp);
            }

            return monsterBotPlayers;
        }

        public List<MonsterBotPlayer> GetNonOKMonsterBotPlayers(List<MonsterBotPlayer> monsterBotPlayers, Sport sport, UserLeague userLeague)
        {
            var nonOKMonsterBotPlayers = new List<MonsterBotPlayer>();
            foreach (var mbp in monsterBotPlayers)
            {
                var nonOK = (from mbc in mbp.MonsterBotPlayerComments where !mbc.IsOK select mbc).ToList();
                if (sport.IsNBA || sport.IsNHL || sport.IsMLB)
                {
                    if (userLeague.LineupFrequency == "W")
                        nonOK.Clear();
                    //else
                    //    nonOK = (from mbc in nonOK
                    //             where mbc.Icon == RotoMonsterIcons.MonsterBotActivate || mbc.Icon == RotoMonsterIcons.MonsterBotActiveApproved
                    //             select mbc).ToList();
                }
                if (nonOK.Count > 0)
                    nonOKMonsterBotPlayers.Add(mbp);
            }

            return nonOKMonsterBotPlayers;
        }

        public List<DisplayPlayer> GetOptimumActivePlayers(
            List<UserLeagueActiveRosterSpot> userLeagueActiveRosterSpots,
            List<UserLeagueTeamPlayer> userLeagueTeamPlayers,
            PlayerType playerType,
            List<OwnershipPlayer> ownershipPlayers,
            Season season,
            List<SeasonPlayer> seasonPlayers,
            List<PositionSourcePlayer> positionSourcePlayers,
            List<PlayerGameState> playerGameStates,
            List<PlayerStatus> playerStatuses,
            List<Game> games,
            bool orderByOwn)
        {
            var rosterPlayers = new List<DisplayPlayer>();

            foreach (var tp in userLeagueTeamPlayers)
            {
                if (tp.IsIR)
                    continue;

                var seasonPlayer = (from sp in seasonPlayers where sp.PlayerId == tp.PlayerId select sp).FirstOrDefault();
                if (seasonPlayer != null && seasonPlayer.PlayerTypeId == playerType.Id)
                {
                    var rosterPlayer = new DisplayPlayer();
                    rosterPlayer.SeasonPlayer = seasonPlayer;
                    rosterPlayer.OwnershipPlayer = (from op in ownershipPlayers where op.PlayerId == tp.PlayerId select op).FirstOrDefault();
                    rosterPlayer.Positions = (from p1 in positionSourcePlayers where p1.PlayerId == seasonPlayer.Player.Id select p1.Position).ToList();
                    rosterPlayer.PlayerGameState = (from p1 in playerGameStates where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                    rosterPlayer.PlayerStatus = (from p1 in playerStatuses where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                    rosterPlayer.Game = (from g in games where g.HomeTeamId == seasonPlayer.TeamId || g.AwayTeamId == seasonPlayer.TeamId select g).FirstOrDefault();
                    rosterPlayer.IsActive = tp.IsActive;
                    rosterPlayers.Add(rosterPlayer);
                }
            }

            if (orderByOwn)
            {
                rosterPlayers = (from op in rosterPlayers
                                 orderby
                                 op.Game != null ? 0 : 1 ascending,
                                 op.Game != null && op.Game.HasStarted && op.IsActive ? 0 : 1 ascending,
                                 op.PlayerGameState == null || (op.PlayerGameState != null && op.PlayerGameState.PlayerGameStateType.IsStarter) ? 1 : 0 descending,
                                 op.OwnershipPlayer == null ? int.MinValue : op.OwnershipPlayer.OwnershipPercent descending
                                 select op).ToList();
            }
            else
            {
                rosterPlayers = (from op in rosterPlayers
                                 orderby
                                 op.Game != null ? 0 : 1 ascending,
                                 op.Game != null && op.Game.HasStarted && op.IsActive ? 0 : 1 ascending,
                                 op.IsActive ? 0 : 1 ascending,
                                 op.PlayerGameState == null || (op.PlayerGameState != null && op.PlayerGameState.PlayerGameStateType.IsStarter) ? 1 : 0 descending,
                                 op.PlayerStatus != null && op.PlayerStatus.PlayerStatusType.UsesDate.GetValueOrDefault(false) ? 0 : 1 ascending,
                                 op.OwnershipPlayer == null ? int.MinValue : op.OwnershipPlayer.ActivePercent descending
                                 select op).ToList();
            }

            string playerTxt = "";
            foreach (var p in rosterPlayers)
            {
                playerTxt += p.SeasonPlayer.Player.ForwardName + ",";
            }

            var rosterHash = new Dictionary<int, List<DisplayPlayer>>();
            foreach (UserLeagueActiveRosterSpot rosterSetting in userLeagueActiveRosterSpots)
            {
                rosterHash[rosterSetting.ActiveRosterSpotId] = new List<DisplayPlayer>();
            }

            foreach (DisplayPlayer player in rosterPlayers)
                AddAutoRosterPlayer(rosterHash, userLeagueActiveRosterSpots, player, null);

            List<DisplayPlayer> outPlayers = new List<DisplayPlayer>();
            foreach (UserLeagueActiveRosterSpot rosterSetting in userLeagueActiveRosterSpots)
            {
                foreach (DisplayPlayer rosterPlayer in rosterHash[rosterSetting.ActiveRosterSpotId])
                {
                    rosterPlayer.IsActive = true;
                    rosterPlayer.ActiveRosterSpot = rosterSetting.ActiveRosterSpot;
                    outPlayers.Add(rosterPlayer);
                }
            }

            return outPlayers;
        }

        public bool AddAutoRosterPlayer(
            Dictionary<int, List<DisplayPlayer>> rosterHash,
            List<UserLeagueActiveRosterSpot> activeRosterSpotSettings,
            DisplayPlayer rosterPlayer,
            Dictionary<int, int> ignoreRosterSpotHash)
        {
            foreach (UserLeagueActiveRosterSpot rosterSetting in activeRosterSpotSettings)
            {
                List<DisplayPlayer> rosterPlayers = rosterHash[rosterSetting.ActiveRosterSpotId];
                if (rosterSetting.ActiveRosterSpot.PositionsQualify(rosterPlayer.Positions))
                {
                    if (rosterPlayers.Count < rosterSetting.NumberOfPlayers)
                    {
                        rosterPlayers.Add(rosterPlayer);
                        return true;
                    }
                }
            }

            // try to move players
            foreach (UserLeagueActiveRosterSpot rosterSetting in activeRosterSpotSettings)
            {
                if (ignoreRosterSpotHash != null && ignoreRosterSpotHash.ContainsKey(rosterSetting.ActiveRosterSpotId))
                    continue;

                List<DisplayPlayer> rosterPlayers = rosterHash[rosterSetting.ActiveRosterSpotId];
                if (rosterSetting.ActiveRosterSpot.PositionsQualify(rosterPlayer.Positions))
                {
                    for (int i = rosterPlayers.Count - 1; i >= 0; i += -1)
                    {
                        DisplayPlayer movePlayer = (DisplayPlayer)rosterPlayers[i];
                        if (PositionsText(movePlayer.Positions) != PositionsText(rosterPlayer.Positions))
                        {
                            Dictionary<int, int> tmpIgnoreHash = new Dictionary<int, int>();
                            if (ignoreRosterSpotHash != null)
                            {
                                foreach (var item in ignoreRosterSpotHash)
                                    tmpIgnoreHash[item.Key] = item.Value;
                            }
                            tmpIgnoreHash[rosterSetting.ActiveRosterSpotId] = 1;
                            if (AddAutoRosterPlayer(rosterHash, activeRosterSpotSettings, movePlayer, tmpIgnoreHash))
                            {
                                rosterPlayers.RemoveAt(i);
                                rosterPlayers.Add(rosterPlayer);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public List<DisplayPlayer> GetRecommendedFreeAgents(
            PlayerType playerType,
            int topCount,
            List<MonsterBarPlayer> monsterBarPlayers,
            List<UserLeagueTeamPlayer> allUserLeagueTeamPlayers,
            List<UserLeagueTeamPlayer> myUserLeagueTeamPlayers,
            List<SeasonPlayer> seasonPlayers,
            List<PlayerStatus> playerStatuses,
            List<PositionSourcePlayer> playerPositions,
            List<OwnershipPlayer> ownershipPlayers
            )
        {
            var topFreeAgents = new List<DisplayPlayer>();

            foreach (var ownershipPlayer in (from op in ownershipPlayers orderby op.OwnershipPercent descending select op))
            {
                var seasonPlayer = (from sp in seasonPlayers where sp.PlayerId == ownershipPlayer.PlayerId select sp).FirstOrDefault();
                if (seasonPlayer == null || seasonPlayer.PlayerTypeId != playerType.Id)
                    continue;

                var monsterBarPlayer = (from p in monsterBarPlayers where p.Player.Id == ownershipPlayer.PlayerId select p).FirstOrDefault();
                if (monsterBarPlayer != null && monsterBarPlayer.IsGoodFreeAgent)
                {
                    var myPlayer = (from p in myUserLeagueTeamPlayers where p.PlayerId == ownershipPlayer.PlayerId select p).FirstOrDefault();
                    var otherPlayer = (from p in allUserLeagueTeamPlayers where p.PlayerId == ownershipPlayer.PlayerId select p).FirstOrDefault();
                    if (myPlayer == null && otherPlayer == null)
                    {
                        var displayPlayer = new DisplayPlayer();
                        displayPlayer.SeasonPlayer = (from sp in seasonPlayers where sp.PlayerId == ownershipPlayer.PlayerId select sp).FirstOrDefault();
                        displayPlayer.PlayerStatus = (from ps in playerStatuses where ps.PlayerId == ownershipPlayer.PlayerId select ps).FirstOrDefault();
                        if (displayPlayer.PlayerStatus != null && displayPlayer.PlayerStatus.PlayerStatusType.Title.Contains("season"))
                            break;
                        displayPlayer.OwnershipPlayer = ownershipPlayer;
                        displayPlayer.Positions = (from p1 in playerPositions where p1.PlayerId == ownershipPlayer.Player.Id select p1.Position).ToList();
                        topFreeAgents.Add(displayPlayer);
                    }
                }
                if (topFreeAgents.Count == topCount)
                    break;
            }

            return topFreeAgents;
        }

        public List<DisplayPlayer> GetTopFreeAgents(
            PlayerType playerType,
            int topCount,
            List<UserLeagueTeamPlayer> allUserLeagueTeamPlayers,
            List<UserLeagueTeamPlayer> myUserLeagueTeamPlayers,
            List<SeasonPlayer> seasonPlayers,
            List<PlayerStatus> playerStatuses,
            List<PositionSourcePlayer> playerPositions,
            List<OwnershipPlayer> ownershipPlayers
            )
        {
            var topFreeAgents = new List<DisplayPlayer>();

            UserLeagueTeamPlayer lowTeamPlayer = null;
            double lowOwnershipPercent = Double.MaxValue;
            foreach (var p in myUserLeagueTeamPlayers)
            {
                var seasonPlayer = (from sp in seasonPlayers where sp.PlayerId == p.PlayerId select sp).FirstOrDefault();
                if (seasonPlayer != null && seasonPlayer.PlayerTypeId == playerType.Id)
                {
                    var ownershipPlayer = (from op in ownershipPlayers where op.PlayerId == p.PlayerId select op).FirstOrDefault();
                    if (ownershipPlayer != null)
                    {
                        if (lowTeamPlayer == null || ownershipPlayer.OwnershipPercent < lowOwnershipPercent)
                        {
                            lowTeamPlayer = p;
                            lowOwnershipPercent = ownershipPlayer.OwnershipPercent;
                        }

                    }
                }
            }

            if (lowTeamPlayer != null)
            {
                foreach (var ownershipPlayer in (from op in ownershipPlayers orderby op.OwnershipPercent descending, op.ActivePercent descending select op))
                {
                    var seasonPlayer = (from sp in seasonPlayers where sp.PlayerId == ownershipPlayer.PlayerId select sp).FirstOrDefault();
                    if (seasonPlayer != null && seasonPlayer.PlayerTypeId == playerType.Id)
                    {
                        var teamPlayer = (from tp in allUserLeagueTeamPlayers where tp.PlayerId == ownershipPlayer.PlayerId select tp).FirstOrDefault();
                        if (teamPlayer == null)
                        {
                            if (ownershipPlayer.OwnershipPercent > lowOwnershipPercent)
                            {
                                var displayPlayer = new DisplayPlayer();
                                displayPlayer.SeasonPlayer = (from sp in seasonPlayers where sp.PlayerId == ownershipPlayer.PlayerId select sp).FirstOrDefault();
                                displayPlayer.PlayerStatus = (from ps in playerStatuses where ps.PlayerId == ownershipPlayer.PlayerId select ps).FirstOrDefault();
                                displayPlayer.OwnershipPlayer = ownershipPlayer;
                                displayPlayer.Positions = (from p1 in playerPositions where p1.PlayerId == ownershipPlayer.Player.Id select p1.Position).ToList();
                                topFreeAgents.Add(displayPlayer);
                            }
                        }

                        if (topFreeAgents.Count == topCount)
                            break;
                    }
                }
            }

            return topFreeAgents;
        }

        public string PositionsText(List<Position> positions)
        {
            var outString = "";
            foreach (var p in positions)
                outString += "," + p.Abbreviation;

            return outString;
        }

    }
}
