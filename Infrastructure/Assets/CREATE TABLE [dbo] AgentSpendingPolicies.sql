CREATE TABLE [dbo].[AgentSpendingPolicies] (
    -- Unique identifier for this spending policy.
    [Id] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_AgentSpendingPolicies_Id] DEFAULT NEWID(),

    -- Agent Account that owns this spending policy.
    --
    -- An Agent Account can have at most one current spending policy.
    [AgentAccountId] UNIQUEIDENTIFIER NOT NULL,

    -- Optional expiration time for the entire policy.
    --
    -- NULL means the policy does not expire automatically.
    [ExpiresAt] DATETIMEOFFSET(7) NULL,

    -- Record creation timestamp.
    [DateCreated] DATETIMEOFFSET(7) NOT NULL
        CONSTRAINT [DF_AgentSpendingPolicies_DateCreated]
        DEFAULT SYSDATETIMEOFFSET(),

    -- Last update timestamp.
    [DateUpdated] DATETIMEOFFSET(7) NULL,

    -- Soft delete timestamp.
    [DeletedAt] DATETIMEOFFSET(7) NULL,

    CONSTRAINT [PK_AgentSpendingPolicies]
        PRIMARY KEY CLUSTERED ([Id]),

    CONSTRAINT [FK_AgentSpendingPolicies_AgentAccounts_AgentAccountId]
        FOREIGN KEY ([AgentAccountId])
        REFERENCES [dbo].[AgentAccounts] ([Id])
);

-- An Agent Account can have at most one current spending policy.
CREATE UNIQUE INDEX [UX_AgentSpendingPolicies_AgentAccountId]
ON [dbo].[AgentSpendingPolicies] ([AgentAccountId])
WHERE [DeletedAt] IS NULL;