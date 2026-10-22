CREATE TABLE [dbo].[AgentPlatformConnections] (

    -- Unique identifier for this connection.
    [Id] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_AgentPlatformConnections_Id] DEFAULT NEWID(),

    -- Agent Account authorized for this AI Platform.
    [AgentAccountId] UNIQUEIDENTIFIER NOT NULL,

    -- UCP Profile URL of the authorized AI Platform.
    [PlatformProfileUrl] NVARCHAR(512)
        COLLATE Latin1_General_100_BIN2 NOT NULL,

    -- 1 = Active
    -- 2 = Suspended
    -- 3 = Revoked
    [Status] TINYINT NOT NULL
        CONSTRAINT [DF_AgentPlatformConnections_Status] DEFAULT (1),

    -- Expiration of the connection authorization.
    [ExpiresAt] DATETIMEOFFSET(7) NOT NULL,

    CONSTRAINT [PK_AgentPlatformConnections]
        PRIMARY KEY CLUSTERED ([Id]),

    CONSTRAINT [FK_AgentPlatformConnections_AgentAccounts]
        FOREIGN KEY ([AgentAccountId])
        REFERENCES [dbo].[AgentAccounts] ([Id]),

    CONSTRAINT [CK_AgentPlatformConnections_Status]
        CHECK ([Status] IN (1, 2, 3)),

    CONSTRAINT [UQ_AgentPlatformConnections]
        UNIQUE ([AgentAccountId], [PlatformProfileUrl])
);