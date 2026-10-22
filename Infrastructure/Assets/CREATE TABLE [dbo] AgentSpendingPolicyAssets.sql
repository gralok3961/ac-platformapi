CREATE TABLE [dbo].[AgentSpendingPolicyAssets] (
    -- Unique identifier for this asset spending rule.
    [Id] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_AgentSpendingPolicyAssets_Id] DEFAULT NEWID(),

    -- Parent spending policy.
    [AgentSpendingPolicyId] UNIQUEIDENTIFIER NOT NULL,

    -- Exact asset currency/code.
    [CurrencyCode] NVARCHAR(80)
        COLLATE Latin1_General_100_BIN2 NOT NULL,

    -- NULL for native assets such as XRP.
    -- Required for issued assets such as RLUSD.
    [Issuer] NVARCHAR(200)
        COLLATE Latin1_General_100_BIN2 NULL,

    -- Maximum amount allowed for a single transaction.
    [PerTransactionLimit] DECIMAL(38, 18) NOT NULL,

    [DateCreated] DATETIMEOFFSET(7) NOT NULL
        CONSTRAINT [DF_AgentSpendingPolicyAssets_DateCreated]
        DEFAULT SYSDATETIMEOFFSET(),

    [DateUpdated] DATETIMEOFFSET(7) NULL,

    [DeletedAt] DATETIMEOFFSET(7) NULL,

    CONSTRAINT [PK_AgentSpendingPolicyAssets]
        PRIMARY KEY CLUSTERED ([Id]),

    CONSTRAINT [FK_AgentSpendingPolicyAssets_AgentSpendingPolicies_AgentSpendingPolicyId]
        FOREIGN KEY ([AgentSpendingPolicyId])
        REFERENCES [dbo].[AgentSpendingPolicies] ([Id]),

    CONSTRAINT [CK_AgentSpendingPolicyAssets_PerTransactionLimit]
        CHECK ([PerTransactionLimit] > 0)
);

CREATE UNIQUE INDEX [UX_AgentSpendingPolicyAssets_Policy_Asset]
ON [dbo].[AgentSpendingPolicyAssets] (
    [AgentSpendingPolicyId],
    [CurrencyCode],
    [Issuer]
)
WHERE [DeletedAt] IS NULL;

CREATE INDEX [IX_AgentSpendingPolicyAssets_AgentSpendingPolicyId]
ON [dbo].[AgentSpendingPolicyAssets] ([AgentSpendingPolicyId])
WHERE [DeletedAt] IS NULL;