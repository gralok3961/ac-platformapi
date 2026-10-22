CREATE TABLE [dbo].[AgentSigningAuthorities] (
    -- Unique identifier for this delegated authority record.
    [Id] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_AgentSigningAuthorities_Id] DEFAULT NEWID(),

    -- Agent Account that owns this delegated authority configuration.
    [AgentAccountId] UNIQUEIDENTIFIER NOT NULL,
    -- Infrastructure provider holding the protected signing key.
    --
    -- 1 = Azure Key Vault
    --
    -- Keep this as an enum-style field so another provider can be added later
    -- without changing the table structure.
    [Provider] TINYINT NOT NULL,

    -- Provider-specific reference used by Hashtrion Signing API
    -- to locate the protected key.
    --
    -- Vault this may be the Key Vault key identifier/reference.
    [ProviderKeyReference] VARCHAR(500) NOT NULL,

    -- Public key associated with the protected authority key.
    --
    -- This is public cryptographic information and may be used
    -- for verification or deriving a network-specific authority identifier.
    [PublicKey] NVARCHAR(300) NOT NULL,

    -- Public network-specific identifier representing the delegated authority.
    --
    -- For XRPL this will currently be the r... address derived from
    -- the KMS public key and later registered as the account's RegularKey.
    --
    -- This maps conceptually to the Wallet-side:
    -- delegatedAuthorityIdentifier
    --
  [DelegatedAuthorityIdentifier] NVARCHAR(200) NOT NULL,

    -- Lifecycle state of this delegated authority.
    --
    -- 1 = Provisioned
    --     Protected key has been created, but the blockchain account
    --     has not yet been confirmed as using this authority.
    --
    -- 2 = Active
    --     Delegated authority has been successfully configured
    --     and verified for the Agent Account.
    --
    -- 3 = Revoked
    --     Authority is no longer allowed to operate for this Agent Account.
    [Status] TINYINT NOT NULL
        CONSTRAINT [DF_AgentSigningAuthorities_Status] DEFAULT (1),

    -- Time when this authority was confirmed as active.
    [ActivatedAt] DATETIMEOFFSET(7) NULL,

    -- Time when this authority was revoked/replaced.
    [RevokedAt] DATETIMEOFFSET(7) NULL,

    -- Record creation timestamp.
    [DateCreated] DATETIMEOFFSET(7) NOT NULL
        CONSTRAINT [DF_AgentSigningAuthorities_DateCreated] DEFAULT SYSDATETIMEOFFSET(),

    -- Last update timestamp.
    [DateUpdated] DATETIMEOFFSET(7) NULL,

    -- Soft delete timestamp.
    [DeletedAt] DATETIMEOFFSET(7) NULL,

    CONSTRAINT [PK_AgentSigningAuthorities]
        PRIMARY KEY CLUSTERED ([Id]),

    CONSTRAINT [FK_AgentSigningAuthorities_AgentAccounts_AgentAccountId]
        FOREIGN KEY ([AgentAccountId])
        REFERENCES [dbo].[AgentAccounts] ([Id])
);

-- Helps retrieve authority history/configuration for an Agent Account.
CREATE INDEX [IX_AgentSigningAuthorities_AgentAccountId]
ON [dbo].[AgentSigningAuthorities] ([AgentAccountId])
WHERE [DeletedAt] IS NULL;

-- An Agent Account should have at most one current signing authority.
-- Provisioned and Active are both considered current; Revoked authorities are retained as history.
CREATE UNIQUE INDEX [UX_AgentSigningAuthorities_Current_AgentAccountId]
ON [dbo].[AgentSigningAuthorities] ([AgentAccountId])
WHERE [Status] IN (1, 2)
  AND [DeletedAt] IS NULL;

-- Provider key references should uniquely identify a protected key.
CREATE UNIQUE INDEX [UX_AgentSigningAuthorities_ProviderKeyReference]
ON [dbo].[AgentSigningAuthorities] ([ProviderKeyReference])
WHERE [DeletedAt] IS NULL;

-- Makes lookup by the public delegated authority identifier efficient.
CREATE INDEX [IX_AgentSigningAuthorities_DelegatedAuthorityIdentifier]
ON [dbo].[AgentSigningAuthorities] ([DelegatedAuthorityIdentifier])
WHERE [DeletedAt] IS NULL;