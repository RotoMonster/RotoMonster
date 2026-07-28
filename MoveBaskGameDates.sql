
UPDATE rmGame SET game_date=DATEADD(month,1,game_date), daytime_date=DATEADD(month,1,daytime_date)
WHERE data_set_id=100 AND is_finished IS NULL
