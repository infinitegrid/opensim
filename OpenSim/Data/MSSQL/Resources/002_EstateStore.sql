BEGIN TRANSACTION

ALTER TABLE dbo.estate_managers DROP CONSTRAINT PK_estate_managers

CREATE NONCLUSTERED INDEX IX_estate_managers ON dbo.estate_managers
	(
	EstateID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.estate_groups DROP CONSTRAINT PK_estate_groups

CREATE NONCLUSTERED INDEX IX_estate_groups ON dbo.estate_groups
	(
	EstateID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


ALTER TABLE dbo.estate_users DROP CONSTRAINT PK_estate_users

CREATE NONCLUSTERED INDEX IX_estate_users ON dbo.estate_users
	(
	EstateID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

COMMIT