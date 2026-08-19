using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RotoMonster.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Data
{
    public class RMDBContext : DbContext
    {

        public RMDBContext(DbContextOptions<RMDBContext> options)
            : base(options)
        {
        }

        public DbSet<Sport> Sports { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerAlias> PlayerAliases { get; set; }
        public DbSet<FantasyProvider> FantasyProviders { get; set; }
        public DbSet<FantasyProviderPlayer> FantasyProviderPlayers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamAlias> TeamAliases { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<SeasonPlayer> SeasonPlayers { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<PlayerType> PlayerTypes { get; set; }
        public DbSet<PlayerGameMissed> PlayedGamesMissed { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<PerValue> PerValues { get; set; }
        public DbSet<NBAPlayerGame> NBAPlayerGames { get; set; }
        public DbSet<MLBHitterGame> MLBHitterGames { get; set; }
        public DbSet<MLBPitcherGame> MLBPitcherGames { get; set; }

        public DbSet<NFLOffensiveGame> NFLOffensiveGame { get; set; }
        public DbSet<NFLKickerGame> NFLKickerGames { get; set; }
        public DbSet<NFLDefenseGame> NFLDefenseGames { get; set; }
        public DbSet<NFLGame> NFLGames { get; set; }

        public DbSet<NHLGoalieGame> NHLGoalieGames { get; set; }
        public DbSet<NHLSkaterGame> NHLSkaterGames { get; set; }

        public DbSet<PlayerDefaultPosition> PlayerDefaultPositions { get; set; }
        public DbSet<ActiveRosterSpot> ActiveRosterSpots { get; set; }
        public DbSet<ActiveRosterSpotPosition> ActiveRosterSpotPositions { get; set; }
        public DbSet<UserLeague> UserLeagues { get; set; }
        public DbSet<UserLeagueTeam> UserLeagueTeams { get; set; }
        public DbSet<UserLeagueTeamPlayer> UserLeagueTeamPlayers { get; set; }
        public DbSet<UserLeagueActiveRosterSpot> UserLeagueActiveRosterSpots { get; set; }
        public DbSet<UserLeagueCategory> UserLeagueCategories { get; set; }
        public DbSet<UserLeagueMissingPlayer> UserLeagueMissingPlayers { get; set; }
        public DbSet<UserLeaguePlayerType> UserLeaguePlayerTypes { get; set; }
        public DbSet<UserLeagueImportError> UserLeagueImportErrors { get; set; }
        public DbSet<UserLeagueWaiverPlayer> UserLeagueWaiverPlayers { get; set; }

        public DbSet<DisplayCategory> DisplayCategories { get; set; }
        public DbSet<PositionSource> PositionSources { get; set; }
        public DbSet<PositionSourcePosition> PositionSourcePositions { get; set; }
        public DbSet<PositionSourcePlayer> PositionSourcePlayers { get; set; }
        public DbSet<Draft> Drafts { get; set; }
        public DbSet<DraftPlayerType> DraftPlayerTypes { get; set; }

        public DbSet<DraftPlayer> DraftPlayers { get; set; }
        public DbSet<OwnershipPlayer> OwnershipPlayers { get; set; }
        public DbSet<CategoryPerValue> CategoryPerValues { get; set; }
        public DbSet<UserDisplayCategory> UserDisplayCategories { get; set; }
        public DbSet<PlayerInjury> PlayerInjuries { get; set; }
        public DbSet<ExtraAnalysisLeague> ExtraAnalysisLeagues { get; set; }
        public DbSet<PlayerStatusType> PlayerStatusTypes { get; set; }
        public DbSet<PlayerStatus> PlayerStatuses { get; set; }
        public DbSet<PlayerStatusTagType> PlayerStatusTagTypes { get; set; }

        public DbSet<PlayerGameStateType> PlayerGameStateTypes { get; set; }
        public DbSet<PlayerGameState> PlayerGameStates { get; set; }
        public DbSet<CompletedTask> CompletedTasks { get; set; }
        public DbSet<CategoriesString> CategoriesStrings { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<GameScoringAlert> GameScoringAlerts { get; set; }
        public DbSet<PlayerGamePositionCategory> PlayerGamePositionCategories { get; set; }
        public DbSet<UserOptionType> UserOptionTypes { get; set; }
        public DbSet<UserOption> UserOptions { get; set; }
        public DbSet<Helper> Helpers { get; set; }
        public DbSet<Tutorial> Tutorials { get; set; }
        public DbSet<TutorialSection> TutorialSections { get; set; }
        public DbSet<TutorialStep> TutorialSteps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sport>().HasNoKey();

            modelBuilder.Entity<Tutorial>().HasIndex(t => t.TutorialKey).IsUnique();

            modelBuilder.Entity<Tutorial>()
                .HasMany(t => t.TutorialSections)
                .WithOne(s => s.Tutorial)
                .HasForeignKey(s => s.TutorialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tutorial>()
                .HasMany(t => t.TutorialSteps)
                .WithOne(s => s.Tutorial)
                .HasForeignKey(s => s.TutorialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeasonTeam>().HasKey(s => new { s.SeasonId, s.TeamId });
            modelBuilder.Entity<SeasonTeam>().ToTable("SeasonTeams");

            modelBuilder.Entity<SeasonDivision>().HasKey(s => new { s.SeasonId, s.DivisionId });
            modelBuilder.Entity<SeasonDivision>().ToTable("SeasonDivisions");

            modelBuilder.Entity<SeasonPlayer>().HasKey(s => new { s.SeasonId, s.PlayerId, s.PlayerTypeId });
            modelBuilder.Entity<SeasonPlayer>().ToTable("SeasonPlayers");

            modelBuilder.Entity<PlayerDefaultPosition>().HasKey(s => new { s.PlayerId, s.PositionId });
            modelBuilder.Entity<PlayerDefaultPosition>().ToTable("PlayerDefaultPositions");

            modelBuilder.Entity<PlayerGameMissed>().HasKey(s => new { s.GameId, s.PlayerId });
            modelBuilder.Entity<PlayerGameMissed>().ToTable("PlayedGamesMissed");

            modelBuilder.Entity<NBAPlayerGame>().HasKey(s => new { s.PlayerId, s.GameId });
            modelBuilder.Entity<NBAPlayerGame>().ToTable("NBAPlayerGames");

            modelBuilder.Entity<MLBHitterGame>().HasKey(s => new { s.PlayerId, s.GameId });
            modelBuilder.Entity<MLBHitterGame>().ToTable("MLBHitterGames");

            modelBuilder.Entity<MLBPitcherGame>().HasKey(s => new { s.PlayerId, s.GameId });
            modelBuilder.Entity<MLBPitcherGame>().ToTable("MLBPitcherGames");

            modelBuilder.Entity<NFLOffensiveGame>().HasKey(s => new { s.PlayerId, s.GameId });
            modelBuilder.Entity<NFLOffensiveGame>().ToTable("NFLOffensiveGames");

            modelBuilder.Entity<NFLKickerGame>().HasKey(s => new { s.PlayerId, s.GameId });
            modelBuilder.Entity<NFLKickerGame>().ToTable("NFLKickerGames");

            modelBuilder.Entity<NFLDefenseGame>().HasKey(s => new { s.PlayerId, s.GameId });
            modelBuilder.Entity<NFLDefenseGame>().ToTable("NFLDefenseGames");

            modelBuilder.Entity<NHLGoalieGame>().HasKey(s => new { s.PlayerId, s.GameId });
            modelBuilder.Entity<NHLGoalieGame>().ToTable("NHLGoalieGames");

            modelBuilder.Entity<NHLSkaterGame>().HasKey(s => new { s.PlayerId, s.GameId });
            modelBuilder.Entity<NHLSkaterGame>().ToTable("NHLSkaterGames");

            modelBuilder.Entity<UserLeagueTeamPlayer>().HasKey(s => new { s.UserLeagueTeamId, s.PlayerId });
            modelBuilder.Entity<UserLeagueTeamPlayer>().ToTable("UserLeagueTeamPlayers");

            modelBuilder.Entity<ActiveRosterSpotPosition>().HasKey(s => new { s.ActiveRosterSpotId, s.PositionId });
            modelBuilder.Entity<ActiveRosterSpotPosition>().ToTable("ActiveRosterSpotPositions");

            modelBuilder.Entity<UserLeagueActiveRosterSpot>().HasKey(s => new { s.UserLeagueId, s.ActiveRosterSpotId });
            modelBuilder.Entity<UserLeagueActiveRosterSpot>().ToTable("UserLeagueActiveRosterSpots");

            modelBuilder.Entity<UserLeagueCategory>().HasKey(s => new { s.UserLeagueId, s.CategoryId });
            modelBuilder.Entity<UserLeagueCategory>().ToTable("UserLeagueCategories");

            modelBuilder.Entity<UserLeagueMissingPlayer>().HasKey(s => new { s.UserLeagueId, s.ProviderId });
            modelBuilder.Entity<UserLeagueMissingPlayer>().ToTable("UserLeagueMissingPlayers");

            modelBuilder.Entity<UserLeagueWaiverPlayer>().HasKey(s => new { s.UserLeagueId, s.PlayerId });
            modelBuilder.Entity<UserLeagueWaiverPlayer>().ToTable("UserLeagueWaiverPlayers");

            modelBuilder.Entity<PositionSourcePosition>().HasKey(s => new { s.PositionSourceId, s.PositionId });
            modelBuilder.Entity<PositionSourcePosition>().ToTable("PositionSourcePositions");

            modelBuilder.Entity<PositionSourcePlayer>().HasKey(s => new { s.SeasonId, s.PositionSourceId, s.PlayerId, s.PositionId });
            modelBuilder.Entity<PositionSourcePlayer>().ToTable("PositionSourcePlayers");

            modelBuilder.Entity<DraftPlayerType>().HasKey(s => new { s.DraftId, s.PlayerTypeId });
            modelBuilder.Entity<DraftPlayerType>().ToTable("DraftPlayerTypes");

            modelBuilder.Entity<DraftPlayer>().HasKey(s => new { s.DraftId, s.PlayerId });
            modelBuilder.Entity<DraftPlayer>().ToTable("DraftPlayers");

            modelBuilder.Entity<OwnershipPlayer>().HasKey(s => new { s.GameDate, s.PlayerId, s.CategoriesStringId });
            modelBuilder.Entity<OwnershipPlayer>().ToTable("OwnershipPlayers");

            modelBuilder.Entity<CategoryPerValue>().HasKey(s => new { s.CategoryId, s.PerValueId });
            modelBuilder.Entity<CategoryPerValue>().ToTable("CategoryPerValues");

            modelBuilder.Entity<UserDisplayCategory>().HasKey(s => new { s.UserId, s.CategoryId });
            modelBuilder.Entity<UserDisplayCategory>().ToTable("UserDisplayCategories");

            modelBuilder.Entity<FantasyProviderPlayer>().HasKey(s => new { s.FantasyProviderId, s.PlayerId });
            modelBuilder.Entity<FantasyProviderPlayer>().ToTable("FantasyProviderPlayers");

            modelBuilder.Entity<UserLeaguePlayerType>().HasKey(s => new { s.UserLeagueId, s.PlayerTypeId });
            modelBuilder.Entity<UserLeaguePlayerType>().ToTable("UserLeaguePlayerTypes");

            modelBuilder.Entity<UserLeagueImportError>().HasKey(s => new { s.UserLeagueId, s.Error });
            modelBuilder.Entity<UserLeagueImportError>().ToTable("UserLeagueImportErrors");

            modelBuilder.Entity<ExtraAnalysisLeague>().HasKey(s => new { s.FantasyProviderId, s.ProviderId });
            modelBuilder.Entity<ExtraAnalysisLeague>().ToTable("ExtraAnalysisLeagues");

            modelBuilder.Entity<TeamAlias>().HasKey(s => new { s.TeamId, s.Alias });
            modelBuilder.Entity<TeamAlias>().ToTable("TeamAliases");

            modelBuilder.Entity<PlayerGameState>().HasKey(s => new { s.GameId, s.PlayerId });
            modelBuilder.Entity<PlayerGameState>().ToTable("PlayerGameStates");

            modelBuilder.Entity<ArticleGame>().HasKey(s => new { s.ArticleId, s.GameId });
            modelBuilder.Entity<ArticleGame>().ToTable("ArticleGames");

            modelBuilder.Entity<ArticlePlayer>().HasKey(s => new { s.ArticleId, s.PlayerId });
            modelBuilder.Entity<ArticlePlayer>().ToTable("ArticlePlayers");

            modelBuilder.Entity<ArticleTeam>().HasKey(s => new { s.ArticleId, s.TeamId });
            modelBuilder.Entity<ArticleTeam>().ToTable("ArticleTeams");

            modelBuilder.Entity<PlayerGamePositionCategory>().HasKey(s => new { s.PlayerId, s.GameId, s.TeamId, s.PositionId, s.CategoryId });
            modelBuilder.Entity<PlayerGamePositionCategory>().ToTable("PlayerGamePositionCategories");

            modelBuilder.Entity<UserOption>().HasKey(s => new { s.UserId, s.UserOptionTypeId });
            modelBuilder.Entity<UserOption>().ToTable("UserOptions");
        }
    }
}
