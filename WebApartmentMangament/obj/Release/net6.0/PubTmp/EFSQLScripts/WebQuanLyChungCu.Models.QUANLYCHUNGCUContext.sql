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
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [Building] (
        [BuildingId] int NOT NULL IDENTITY,
        [BuildingName] nvarchar(max) NULL,
        [BuildingCode] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [Zip] nvarchar(max) NULL,
        [FloorNumber] int NULL,
        [ApartmentNumber] int NULL,
        [AccNumber] int NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_Building] PRIMARY KEY ([BuildingId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [InFo] (
        [InfoId] int NOT NULL IDENTITY,
        [FullName] nvarchar(max) NULL,
        [BirthDay] datetime2 NULL,
        [Sex] tinyint NULL,
        [CMND_CCCD] varchar(50) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [Country] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [District] nvarchar(max) NULL,
        [Ward] nvarchar(max) NULL,
        [StreetAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_InFo] PRIMARY KEY ([InfoId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [News] (
        [NewsId] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NULL,
        [Slug] nvarchar(max) NULL,
        [Image] nvarchar(max) NULL,
        [description] ntext NULL,
        [CreateDay] date NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_News] PRIMARY KEY ([NewsId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [Relationships] (
        [RelationshipId] int NOT NULL IDENTITY,
        [RelationshipName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Relationships] PRIMARY KEY ([RelationshipId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [Role] (
        [RoleId] int NOT NULL IDENTITY,
        [RoleName] nvarchar(max) NULL,
        CONSTRAINT [PK_Role] PRIMARY KEY ([RoleId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [Service] (
        [ServiceId] int NOT NULL IDENTITY,
        [ServiceName] nvarchar(max) NULL,
        [description] nvarchar(max) NULL,
        [ServiceFee] decimal(18,0) NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_Service] PRIMARY KEY ([ServiceId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [Apartment] (
        [ApartmentId] int NOT NULL IDENTITY,
        [BuildingId] int NULL,
        [ApartmentCode] varchar(50) NULL,
        [ApartmentName] nvarchar(max) NULL,
        [ApartmentNumber] int NULL,
        [FloorNumber] int NULL,
        [StartDay] datetime2 NULL,
        [Area] float NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_Apartment] PRIMARY KEY ([ApartmentId]),
        CONSTRAINT [FK_CanHo_ChungCu] FOREIGN KEY ([BuildingId]) REFERENCES [Building] ([BuildingId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [Account] (
        [AccountId] int NOT NULL IDENTITY,
        [ApartmentId] int NULL,
        [Code] nvarchar(max) NULL,
        [Avartar] varchar(50) NULL,
        [UserName] nvarchar(max) NULL,
        [Password] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [InfoId] int NULL,
        [RoleId] int NULL,
        [RelationshipId] int NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_Account] PRIMARY KEY ([AccountId]),
        CONSTRAINT [FK_Account_Apartment] FOREIGN KEY ([ApartmentId]) REFERENCES [Apartment] ([ApartmentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Account_InFo] FOREIGN KEY ([InfoId]) REFERENCES [InFo] ([InfoId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Account_Relationships_RelationshipId] FOREIGN KEY ([RelationshipId]) REFERENCES [Relationships] ([RelationshipId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Account_Role] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([RoleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [Apartment_Service] (
        [ApartmentId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [StartDay] date NULL,
        [EndDay] date NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_Apartment_Service] PRIMARY KEY ([ApartmentId], [ServiceId]),
        CONSTRAINT [FK_Apartment_Service_Apartment] FOREIGN KEY ([ApartmentId]) REFERENCES [Apartment] ([ApartmentId]),
        CONSTRAINT [FK_Apartment_Service_Service] FOREIGN KEY ([ServiceId]) REFERENCES [Service] ([ServiceId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [ElectricMeter] (
        [ElectricMeterId] int NOT NULL IDENTITY,
        [ApartmentId] int NULL,
        [RegistrationDate] date NULL,
        [Code] varchar(max) NULL,
        [DeadingDate] date NULL,
        [NumberOne] float NULL,
        [NumberEnd] float NULL,
        [Price] decimal(18,0) NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_ElectricMeter] PRIMARY KEY ([ElectricMeterId]),
        CONSTRAINT [FK_ElectricMeter_Apartment] FOREIGN KEY ([ApartmentId]) REFERENCES [Apartment] ([ApartmentId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [ResidentsRequired] (
        [RequestId] int NOT NULL IDENTITY,
        [AccountId] int NULL,
        [ApartmentId] int NULL,
        [Title] nvarchar(max) NULL,
        [description] nvarchar(max) NULL,
        [CreateDay] datetime2 NULL,
        [FixDay] datetime2 NULL,
        [Pending] int NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_ResidentsRequired] PRIMARY KEY ([RequestId]),
        CONSTRAINT [FK_ResidentsRequired_Apartment] FOREIGN KEY ([ApartmentId]) REFERENCES [Apartment] ([ApartmentId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [WaterMeter] (
        [WaterMeterId] int NOT NULL IDENTITY,
        [ApartmentId] int NULL,
        [RegistrationDate] datetime2 NULL,
        [Code] varchar(max) NULL,
        [DeadingDate] datetime2 NULL,
        [NumberOne] float NULL,
        [NumberEnd] float NULL,
        [Price] decimal(18,0) NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_WaterMeter] PRIMARY KEY ([WaterMeterId]),
        CONSTRAINT [FK_WaterMeter_Apartment] FOREIGN KEY ([ApartmentId]) REFERENCES [Apartment] ([ApartmentId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [Contract] (
        [ContractId] int NOT NULL IDENTITY,
        [ApartmentId] int NULL,
        [AccountId] int NULL,
        [StartDay] date NULL,
        [EndDay] date NULL,
        [Monthly_rent] decimal(18,0) NULL,
        [Deposit] decimal(18,0) NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_Contract] PRIMARY KEY ([ContractId]),
        CONSTRAINT [FK_Contract_Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Account] ([AccountId]),
        CONSTRAINT [FK_Contract_Apartment] FOREIGN KEY ([ApartmentId]) REFERENCES [Apartment] ([ApartmentId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [History] (
        [HistoryId] int NOT NULL IDENTITY,
        [AccountId] int NULL,
        [Day] date NULL,
        [description] nvarchar(max) NULL,
        [Action] tinyint NULL,
        [Screen] nvarchar(max) NULL,
        CONSTRAINT [PK_History] PRIMARY KEY ([HistoryId]),
        CONSTRAINT [FK_History_Account] FOREIGN KEY ([AccountId]) REFERENCES [Account] ([AccountId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE TABLE [Revenue] (
        [RevenueId] int NOT NULL IDENTITY,
        [ApartmentId] int NULL,
        [TotalMoney] decimal(18,0) NULL,
        [Pay] decimal(18,0) NULL,
        [Debt] decimal(18,0) NULL,
        [ServiceFee] decimal(18,0) NULL,
        [CodeVoucher] varchar(max) NULL,
        [WaterNumber] float NULL,
        [ElectricNumber] float NULL,
        [DayCreat] date NULL,
        [DayPay] date NULL,
        [Payments] tinyint NULL,
        [AccountId] int NULL,
        [Status] tinyint NULL,
        CONSTRAINT [PK_Revenue] PRIMARY KEY ([RevenueId]),
        CONSTRAINT [FK_Revenue_Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Account] ([AccountId]),
        CONSTRAINT [FK_Revenue_Apartment] FOREIGN KEY ([ApartmentId]) REFERENCES [Apartment] ([ApartmentId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Account_ApartmentId] ON [Account] ([ApartmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Account_InfoId] ON [Account] ([InfoId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Account_RelationshipId] ON [Account] ([RelationshipId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Account_RoleId] ON [Account] ([RoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Apartment_BuildingId] ON [Apartment] ([BuildingId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Apartment_Service_ServiceId] ON [Apartment_Service] ([ServiceId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Contract_AccountId] ON [Contract] ([AccountId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Contract_ApartmentId] ON [Contract] ([ApartmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_ElectricMeter_ApartmentId] ON [ElectricMeter] ([ApartmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_History_AccountId] ON [History] ([AccountId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_ResidentsRequired_ApartmentId] ON [ResidentsRequired] ([ApartmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Revenue_AccountId] ON [Revenue] ([AccountId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_Revenue_ApartmentId] ON [Revenue] ([ApartmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    CREATE INDEX [IX_WaterMeter_ApartmentId] ON [WaterMeter] ([ApartmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231229153110_init')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231229153110_init', N'6.0.18');
END;
GO

COMMIT;
GO

