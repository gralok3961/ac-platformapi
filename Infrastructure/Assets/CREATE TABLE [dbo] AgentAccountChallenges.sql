-- =========================
-- Agent Account Challenge
-- =========================
CREATE TABLE [dbo].[AgentAccountChallenges]
(
    [Id] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_AgentAccountChallenges_Id]
        DEFAULT NEWSEQUENTIALID(),

    -- Wallet installation that requested the challenge.
    [AppInstallationId] UNIQUEIDENTIFIER NOT NULL,

    -- Public challenge identifier.
    [ChallengeId] NVARCHAR(64) NOT NULL,

    -- Challenge purpose.
    -- 1 = RegisterAgentAccount
    -- 2 = RevokeAgentAccess
    [Purpose] TINYINT NOT NULL,

    -- Blockchain chain id.
    -- 1 = XRPL, 2 = Stellar
    [ChainId] INT NOT NULL,

    -- Agent Account address to verify.
    [PublicAddress] NVARCHAR(150) NOT NULL,

    -- Exact message signed by Wallet.
    [Challenge] NVARCHAR(MAX) NOT NULL,

    -- One-time random nonce.
    [Nonce] NVARCHAR(64) NOT NULL,

    -- 0 = Pending, 1 = Used, 2 = Expired, 3 = Failed
    [Status] TINYINT NOT NULL
        CONSTRAINT [DF_AgentAccountChallenges_Status]
        DEFAULT (0),

    -- Ownership proof.
    [PublicKey] NVARCHAR(300) NULL,
    [Signature] NVARCHAR(MAX) NULL,

    -- Challenge expiry.
    [ExpiresAt] DATETIMEOFFSET(7) NOT NULL,

    -- Ownership verification time.
    [VerifiedAt] DATETIMEOFFSET(7) NULL,

    -- Challenge consumption time.
    [UsedAt] DATETIMEOFFSET(7) NULL,

    [DateCreated] DATETIMEOFFSET(7) NOT NULL
        CONSTRAINT [DF_AgentAccountChallenges_DateCreated]
        DEFAULT SYSDATETIMEOFFSET(),

    [DateUpdated] DATETIMEOFFSET(7) NULL,

    CONSTRAINT [PK_AgentAccountChallenges]
        PRIMARY KEY CLUSTERED ([Id]),

    CONSTRAINT [FK_AgentAccountChallenges_AppInstallations_AppInstallationId]
        FOREIGN KEY ([AppInstallationId])
        REFERENCES [dbo].[AppInstallations] ([Id]),

    CONSTRAINT [UQ_AgentAccountChallenges_ChallengeId]
        UNIQUE ([ChallengeId])
);