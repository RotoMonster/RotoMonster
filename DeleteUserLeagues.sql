delete UserLeagueCategories
delete UserLeaguePlayerTypes
delete UserLeagueActiveRosterSpots
delete UserLeagueTeamPlayers
delete UserLeagueTeams
delete UserLeagues
DBCC CHECKIDENT ('UserLeagues', RESEED, 0)
DBCC CHECKIDENT ('UserLeagueTeams', RESEED, 0)

delete DraftPlayers
delete draftplayertypes
delete Drafts
DBCC CHECKIDENT ('Drafts', RESEED, 0)
