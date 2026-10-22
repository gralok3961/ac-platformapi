CREATE TABLE dbo.RequestNonces
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_RequestNonces PRIMARY KEY
        DEFAULT NEWSEQUENTIALID(),

    Scope NVARCHAR(100) COLLATE Latin1_General_100_BIN2 NOT NULL,

    Nonce NVARCHAR(128) COLLATE Latin1_General_100_BIN2 NOT NULL,

    DateCreated DATETIMEOFFSET(7) NOT NULL
        CONSTRAINT DF_RequestNonces_DateCreated
        DEFAULT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'),

    ExpiresAt DATETIMEOFFSET(7) NOT NULL,

    CONSTRAINT CK_RequestNonces_ExpiresAt
        CHECK (ExpiresAt > DateCreated)
);

CREATE UNIQUE INDEX UX_RequestNonces_Scope_Nonce
ON dbo.RequestNonces (Scope, Nonce);