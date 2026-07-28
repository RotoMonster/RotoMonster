Build started...
Build succeeded.
BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sports]') AND [c].[name] = N'MenuColor');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Sports] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Sports] ALTER COLUMN [MenuColor] nvarchar(10) NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sports]') AND [c].[name] = N'LogoColor');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Sports] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Sports] ALTER COLUMN [LogoColor] nvarchar(10) NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sports]') AND [c].[name] = N'ESPNCode');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Sports] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Sports] ALTER COLUMN [ESPNCode] nvarchar(5) NULL;
GO

ALTER TABLE [Sports] ADD [UseTotalMonsterBar] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [PlayerGameStateTypes] ADD [IsProbableStarter] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230704003332_230703_1', N'6.0.5');
GO

COMMIT;
GO


