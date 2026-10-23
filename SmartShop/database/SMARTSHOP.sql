-- SmartShop Database Setup Script
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'SMARTSHOP')
BEGIN
    CREATE DATABASE [SMARTSHOP];
END;
GO

USE [SMARTSHOP];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO


BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023111911_InitialCreate'
)
BEGIN
    CREATE TABLE [ChatMessage] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] nvarchar(64) NOT NULL,
        [Role] nvarchar(16) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_ChatMessage] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023111911_InitialCreate'
)
BEGIN
    CREATE TABLE [Product] (
        [ProductId] int NOT NULL,
        [ProductName] nvarchar(128) NOT NULL,
        [Category] nvarchar(64) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Features] nvarchar(512) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Product] PRIMARY KEY ([ProductId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023111911_InitialCreate'
)
BEGIN
    CREATE TABLE [Review] (
        [ReviewId] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [CustomerName] nvarchar(64) NOT NULL,
        [Content] nvarchar(1000) NOT NULL,
        [Rating] int NOT NULL,
        [Sentiment] nvarchar(16) NOT NULL,
        [Score] float NOT NULL,
        [Summary] nvarchar(256) NOT NULL,
        [CreatedAt] datetime NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Review] PRIMARY KEY ([ReviewId]),
        CONSTRAINT [FK_Review_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([ProductId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023111911_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ProductId', N'Category', N'Description', N'Features', N'Price', N'ProductName') AND [object_id] = OBJECT_ID(N'[Product]'))
        SET IDENTITY_INSERT [Product] ON;
    EXEC(N'INSERT INTO [Product] ([ProductId], [Category], [Description], [Features], [Price], [ProductName])
    VALUES (1, N''Phone'', NULL, N''6.2 inch AMOLED, Snapdragon 8 Elite, 50MP camera'', 22990000.0, N''Galaxy S25''),
    (2, N''Laptop'', NULL, N''13.6 inch, 16GB RAM, 18 hours battery'', 28490000.0, N''MacBook Air M4''),
    (3, N''Accessory'', NULL, N''Active noise cancellation, USB-C, IP57'', 6790000.0, N''AirPods Pro 3''),
    (4, N''Phone'', NULL, N''120Hz display, 5500mAh battery, 108MP camera'', 6490000.0, N''Xiaomi Redmi Note 15'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ProductId', N'Category', N'Description', N'Features', N'Price', N'ProductName') AND [object_id] = OBJECT_ID(N'[Product]'))
        SET IDENTITY_INSERT [Product] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023111911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatMessage_CreatedAt] ON [ChatMessage] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023111911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatMessage_SessionId] ON [ChatMessage] ([SessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023111911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Review_ProductId] ON [Review] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023111911_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261023111911_InitialCreate', N'10.0.13');
END;

COMMIT;
GO

