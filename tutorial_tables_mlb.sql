BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819145452_TutorialTables'
)
BEGIN
    CREATE TABLE [Tutorials] (
        [Id] int NOT NULL IDENTITY,
        [TutorialKey] nvarchar(64) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Purpose] nvarchar(max) NULL,
        [DisplayOrder] int NOT NULL,
        [IsDisabled] bit NOT NULL,
        [ModifiedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Tutorials] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819145452_TutorialTables'
)
BEGIN
    CREATE TABLE [TutorialSections] (
        [Id] int NOT NULL IDENTITY,
        [TutorialId] int NOT NULL,
        [Heading] nvarchar(200) NOT NULL,
        [Body] nvarchar(max) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL,
        [IsDisabled] bit NOT NULL,
        CONSTRAINT [PK_TutorialSections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TutorialSections_Tutorials_TutorialId] FOREIGN KEY ([TutorialId]) REFERENCES [Tutorials] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819145452_TutorialTables'
)
BEGIN
    CREATE TABLE [TutorialSteps] (
        [Id] int NOT NULL IDENTITY,
        [TutorialId] int NOT NULL,
        [TargetSelector] nvarchar(200) NOT NULL,
        [Title] nvarchar(200) NULL,
        [Body] nvarchar(max) NULL,
        [Placement] nvarchar(10) NULL,
        [DisplayOrder] int NOT NULL,
        [IsDisabled] bit NOT NULL,
        CONSTRAINT [PK_TutorialSteps] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TutorialSteps_Tutorials_TutorialId] FOREIGN KEY ([TutorialId]) REFERENCES [Tutorials] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819145452_TutorialTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tutorials_TutorialKey] ON [Tutorials] ([TutorialKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819145452_TutorialTables'
)
BEGIN
    CREATE INDEX [IX_TutorialSections_TutorialId] ON [TutorialSections] ([TutorialId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819145452_TutorialTables'
)
BEGIN
    CREATE INDEX [IX_TutorialSteps_TutorialId] ON [TutorialSteps] ([TutorialId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819145452_TutorialTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260819145452_TutorialTables', N'10.0.8');
END;

COMMIT;
GO

