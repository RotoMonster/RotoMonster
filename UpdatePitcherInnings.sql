update MLBPitcherGames
set Innings=Innings+(select convert(float,third_innings)/3 from r_mlb..rmBasePitcherPlayerGame pg
where pg.player_id=MLBPitcherGames.PlayerId and pg.game_id=MLBPitcherGames.GameId)
