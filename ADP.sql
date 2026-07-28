SELECT
MAX(p.Id) AS PlayerId
,COUNT(*) AS cnt
,MAX(FirstName) AS FirstName
,MAX(LastName) AS LastName
,AVG(CONVERT(float,DraftOrder)) AS Pick
,MIN(DraftOrder) AS MinPick
,MAX(DraftOrder) AS MaxPick
,STDEV(DraftOrder) AS StdevPick
FROM DraftPlayers dp
INNER JOIN Drafts d ON d.Id=dp.DraftId
INNER JOIN Players p ON p.Id=dp.PlayerId
INNER JOIN Seasons s ON s.Id=d.SeasonId
WHERE dp.Price is null 
and d.DraftDate<s.StartDate
AND d.ProviderLeagueId IN (SELECT ProviderLeagueId FROM UserLeagues WHERE IsAuction=0 AND (IsProLeague=1 OR title like '%yahoo%'))
GROUP BY PlayerId
ORDER BY Pick
