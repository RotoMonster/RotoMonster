Build started...
Build succeeded.
BEGIN TRANSACTION;
DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sports]') AND [c].[name] = N'MenuColor');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Sports] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Sports] ALTER COLUMN [MenuColor] nvarchar(10) NULL;

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sports]') AND [c].[name] = N'LogoColor');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Sports] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Sports] ALTER COLUMN [LogoColor] nvarchar(10) NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sports]') AND [c].[name] = N'ESPNCode');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Sports] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Sports] ALTER COLUMN [ESPNCode] nvarchar(5) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260426132458_260426_1', N'9.0.5');

COMMIT;
GO


