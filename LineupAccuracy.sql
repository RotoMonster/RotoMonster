IF OBJECT_ID('dbo.LineupProjectionSnapshots', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LineupProjectionSnapshots (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SnapshotDate DATE NOT NULL,
        TakenAt DATETIME2 NOT NULL,
        GameId INT NOT NULL,
        TeamId INT NOT NULL,
        PlayerId INT NOT NULL,
        PositionId INT NULL,
        PlayerGameStateTypeId INT NOT NULL,
        Details NVARCHAR(200) NULL
    );
    CREATE UNIQUE INDEX IX_LineupProjectionSnapshots_Snap ON dbo.LineupProjectionSnapshots (SnapshotDate, GameId, PlayerId);
END
GO

IF OBJECT_ID('dbo.SnapshotProjectedLineups', 'P') IS NOT NULL DROP PROCEDURE dbo.SnapshotProjectedLineups;
GO

CREATE PROCEDURE dbo.SnapshotProjectedLineups
    @Days INT = 7
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @today DATE = CAST(GETDATE() AS DATE);

    INSERT INTO dbo.LineupProjectionSnapshots
        (SnapshotDate, TakenAt, GameId, TeamId, PlayerId, PositionId, PlayerGameStateTypeId, Details)
    SELECT @today, SYSUTCDATETIME(), s.GameId, s.TeamId, s.PlayerId, s.PositionId, s.PlayerGameStateTypeId, s.Details
    FROM PlayerGameStates s
    JOIN PlayerGameStateTypes t ON t.Id = s.PlayerGameStateTypeId
    JOIN Games g ON g.Id = s.GameId
    WHERE t.IsProbableStarter = 1
      AND g.GameDate >= @today
      AND g.GameDate < DATEADD(DAY, @Days, @today)
      AND NOT EXISTS (
          SELECT 1 FROM dbo.LineupProjectionSnapshots x
          WHERE x.SnapshotDate = @today AND x.GameId = s.GameId AND x.PlayerId = s.PlayerId);

    SELECT @@ROWCOUNT AS RowsSnapshotted;
END
GO

IF OBJECT_ID('dbo.ProjectedLineupAccuracy', 'P') IS NOT NULL DROP PROCEDURE dbo.ProjectedLineupAccuracy;
GO

CREATE PROCEDURE dbo.ProjectedLineupAccuracy
    @SnapshotDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @SnapshotDate IS NULL
        SELECT @SnapshotDate = MAX(SnapshotDate) FROM dbo.LineupProjectionSnapshots;

    ;WITH graded AS (
        SELECT
            p.SnapshotDate,
            pos.Abbreviation AS Pos,
            CASE WHEN CHARINDEX(',', ISNULL(p.Details, '')) > 0
                 THEN LEFT(p.Details, CHARINDEX(',', p.Details) - 1)
                 ELSE ISNULL(NULLIF(p.Details, ''), '(none)') END AS Src,
            CASE
                WHEN g.IsFinished = 0 THEN 'Not played'
                WHEN EXISTS (
                    SELECT 1 FROM PlayerGameStates a
                    JOIN PlayerGameStateTypes ast ON ast.Id = a.PlayerGameStateTypeId
                    WHERE a.GameId = p.GameId AND a.PlayerId = p.PlayerId AND ast.IsStarter = 1) THEN 'Hit'
                WHEN EXISTS (
                    SELECT 1 FROM PlayerGameStates a
                    JOIN PlayerGameStateTypes ast ON ast.Id = a.PlayerGameStateTypeId
                    WHERE a.GameId = p.GameId AND a.TeamId = p.TeamId AND ast.IsStarter = 1
                      AND (p.PositionId IS NULL OR a.PositionId = p.PositionId)) THEN 'Miss'
                ELSE 'No final lineup'
            END AS Result
        FROM dbo.LineupProjectionSnapshots p
        JOIN Games g ON g.Id = p.GameId
        LEFT JOIN Positions pos ON pos.Id = p.PositionId
        WHERE p.SnapshotDate = @SnapshotDate
    )
    SELECT
        CASE WHEN GROUPING(Pos) = 1 THEN 'All' ELSE ISNULL(Pos, '-') END AS Position,
        CASE WHEN GROUPING(Src) = 1 THEN 'All' ELSE Src END AS Source,
        SUM(CASE WHEN Result = 'Hit' THEN 1 ELSE 0 END) AS Hits,
        SUM(CASE WHEN Result = 'Miss' THEN 1 ELSE 0 END) AS Misses,
        SUM(CASE WHEN Result IN ('Not played', 'No final lineup') THEN 1 ELSE 0 END) AS Ungraded,
        CAST(100.0 * SUM(CASE WHEN Result = 'Hit' THEN 1 ELSE 0 END)
             / NULLIF(SUM(CASE WHEN Result IN ('Hit', 'Miss') THEN 1 ELSE 0 END), 0) AS DECIMAL(5,1)) AS HitPct
    FROM graded
    GROUP BY GROUPING SETS ((Pos, Src), (Pos), ())
    ORDER BY GROUPING(Pos), Pos, GROUPING(Src), Src;

    SELECT
        p.SnapshotDate, g.GameDate, tm.Code AS Team, pos.Abbreviation AS Position,
        pl.FirstName + ' ' + pl.LastName AS Projected, p.Details,
        STUFF((
            SELECT ', ' + apl.FirstName + ' ' + apl.LastName
            FROM PlayerGameStates a
            JOIN PlayerGameStateTypes ast ON ast.Id = a.PlayerGameStateTypeId
            JOIN Players apl ON apl.Id = a.PlayerId
            WHERE a.GameId = p.GameId AND a.TeamId = p.TeamId AND ast.IsStarter = 1
              AND (p.PositionId IS NULL OR a.PositionId = p.PositionId)
            FOR XML PATH('')), 1, 2, '') AS ActualStarter
    FROM dbo.LineupProjectionSnapshots p
    JOIN Games g ON g.Id = p.GameId
    JOIN Teams tm ON tm.Id = p.TeamId
    JOIN Players pl ON pl.Id = p.PlayerId
    LEFT JOIN Positions pos ON pos.Id = p.PositionId
    WHERE p.SnapshotDate = @SnapshotDate
      AND g.IsFinished = 1
      AND NOT EXISTS (
          SELECT 1 FROM PlayerGameStates a
          JOIN PlayerGameStateTypes ast ON ast.Id = a.PlayerGameStateTypeId
          WHERE a.GameId = p.GameId AND a.PlayerId = p.PlayerId AND ast.IsStarter = 1)
    ORDER BY g.GameDate, tm.Code;
END
GO
