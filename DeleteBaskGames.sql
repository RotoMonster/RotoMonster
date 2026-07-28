update rmPlayerUpdate set game_id = null where game_id in (select game_id from rmGame where data_set_id=100 and is_finished is null)

delete rmBaskPlayerDailyProjection where game_id in (select game_id from rmGame where data_set_id=100 and is_finished is null)
delete rmProjectionPlayerGame where game_id in (select game_id from rmGame where data_set_id=100 and is_finished is null)

delete rmDailyScenarioPlayer where daily_scenario_id in (select daily_scenario_id from rmDailyScenario where game_id in (select game_id from rmGame where data_set_id=100 and is_finished is null))
delete rmDailyScenarioUserView where daily_scenario_id in (select daily_scenario_id from rmDailyScenario where game_id in (select game_id from rmGame where data_set_id=100 and is_finished is null))
delete rmDailyScenario where game_id in (select game_id from rmGame where data_set_id=100 and is_finished is null)
delete rmGameOdds where game_id in (select game_id from rmGame where data_set_id=100 and is_finished is null)
delete rmGame where game_id in (select game_id from rmGame where data_set_id=100 and is_finished is null)
