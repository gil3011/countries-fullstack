SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Read all countries
CREATE PROCEDURE sp_Countries2026_ReadAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        Id, Cca3, CommonName, OfficialName, Region, Subregion,
        Latitude, Longitude, AreaKm2, IsLandlocked, [Population],
        FlagUrl, WikipediaUrl
    FROM Countries2026;
END
GO

-- Get by cca3
CREATE PROCEDURE sp_Countries2026_GetByCca3
    @Cca3 NCHAR(3)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        Id, Cca3, CommonName, OfficialName, Region, Subregion,
        Latitude, Longitude, AreaKm2, IsLandlocked, Population,
        FlagUrl, WikipediaUrl
    FROM Countries2026
    WHERE Cca3 = @Cca3;
END
GO

-- Insert country, return new Id
CREATE PROCEDURE sp_Countries2026_Insert
    @Cca3 NCHAR(3),
    @CommonName NVARCHAR(200),
    @OfficialName NVARCHAR(300),
    @Region INT,
    @Subregion NVARCHAR(150) = NULL,
    @Latitude FLOAT = NULL,
    @Longitude FLOAT = NULL,
    @AreaKm2 FLOAT = NULL,
    @IsLandlocked BIT = 0,
    @Population INT = 0,
    @FlagUrl NVARCHAR(1000) = NULL,
    @WikipediaUrl NVARCHAR(1000) = NULL,
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Countries2026
    (Cca3, CommonName, OfficialName, Region, Subregion, Latitude, Longitude, AreaKm2, IsLandlocked, Population, FlagUrl, WikipediaUrl)
    VALUES
    (@Cca3, @CommonName, @OfficialName, @Region, @Subregion, @Latitude, @Longitude, @AreaKm2, @IsLandlocked, @Population, @FlagUrl, @WikipediaUrl);

    SET @NewId = CAST(SCOPE_IDENTITY() AS INT);
END
GO

-- Update country
CREATE PROCEDURE sp_Countries2026_Update
    @Id INT,
    @Cca3 NCHAR(3),
    @CommonName NVARCHAR(200),
    @OfficialName NVARCHAR(300),
    @Region INT,
    @Subregion NVARCHAR(150) = NULL,
    @Latitude FLOAT = NULL,
    @Longitude FLOAT = NULL,
    @AreaKm2 FLOAT = NULL,
    @IsLandlocked BIT = 0,
    @Population INT = 0,
    @FlagUrl NVARCHAR(1000) = NULL,
    @WikipediaUrl NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Countries2026
    SET
        Cca3 = @Cca3,
        CommonName = @CommonName,
        OfficialName = @OfficialName,
        Region = @Region,
        Subregion = @Subregion,
        Latitude = @Latitude,
        Longitude = @Longitude,
        AreaKm2 = @AreaKm2,
        IsLandlocked = @IsLandlocked,
        Population = @Population,
        FlagUrl = @FlagUrl,
        WikipediaUrl = @WikipediaUrl
    WHERE Id = @Id;
END
GO

-- Delete country
ALTER PROCEDURE sp_Countries2026_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Countries2026 WHERE Id = @Id;

    RETURN @@ROWCOUNT;
END
GO


-- Capitals: Insert
CREATE PROCEDURE sp_Capitals2026_Insert
    @CountryId INT,
    @Name NVARCHAR(200),
    @Latitude FLOAT = NULL,
    @Longitude FLOAT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Capitals2026 (CountryId, Name, Latitude, Longitude)
    VALUES (@CountryId, @Name, @Latitude, @Longitude);
END
GO
-- Capitals: Get by country
CREATE PROCEDURE sp_Capitals2026_GetByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, CountryId, Name, Latitude, Longitude
    FROM Capitals2026
    WHERE CountryId = @CountryId;
END
GO

-- Capitals: Delete by country
CREATE PROCEDURE sp_Capitals2026_DeleteByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Capitals2026 WHERE CountryId = @CountryId;
END
GO

-- Languages: get or create
CREATE PROCEDURE sp_Languages2026_GetOrCreate
    @Iso639_1 NCHAR(2) = NULL,
    @LanguageName NVARCHAR(200) = NULL,
    @LanguageId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Iso639_1 IS NOT NULL
    BEGIN
        SELECT @LanguageId = Id FROM Languages2026 WHERE Iso639_1 = @Iso639_1;
    END

    IF @LanguageId IS NULL
    BEGIN
        INSERT INTO Languages2026 (Iso639_1, LanguageName)
        VALUES (@Iso639_1, @LanguageName);

        SET @LanguageId = CAST(SCOPE_IDENTITY() AS INT);
    END
END
GO

-- CountryLanguages: insert relation (ignore if exists)
CREATE PROCEDURE sp_CountryLanguages2026_Insert
    @CountryId INT,
    @LanguageId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM CountryLanguages2026 WHERE CountryId = @CountryId AND LanguageId = @LanguageId)
        INSERT INTO CountryLanguages2026 (CountryId, LanguageId) VALUES (@CountryId, @LanguageId);
END
GO

-- CountryLanguages: get by country
CREATE PROCEDURE sp_CountryLanguages2026_GetByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT l.Id AS LanguageId, l.Iso639_1, l.LanguageName
    FROM CountryLanguages2026 cl
    JOIN Languages2026 l ON cl.LanguageId = l.Id
    WHERE cl.CountryId = @CountryId;
END
GO

-- CountryLanguages: delete by country
CREATE PROCEDURE sp_CountryLanguages2026_DeleteByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM CountryLanguages2026 WHERE CountryId = @CountryId;
END
GO

-- Currencies: get or create
CREATE PROCEDURE sp_Currencies2026_GetOrCreate
    @CurrencyCode NCHAR(3),
    @CurrencyName NVARCHAR(200),
    @CurrencySymbol NVARCHAR(50) = NULL,
    @CurrencyId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @CurrencyId = Id FROM Currencies2026 WHERE CurrencyCode = @CurrencyCode;
    IF @CurrencyId IS NULL
    BEGIN
        INSERT INTO Currencies2026 (CurrencyCode, CurrencyName, CurrencySymbol)
        VALUES (@CurrencyCode, @CurrencyName, @CurrencySymbol);
        SET @CurrencyId = CAST(SCOPE_IDENTITY() AS INT);
    END
END
GO

-- CountryCurrencies: insert relation
CREATE PROCEDURE sp_CountryCurrencies2026_Insert
    @CountryId INT,
    @CurrencyId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM CountryCurrencies2026 WHERE CountryId = @CountryId AND CurrencyId = @CurrencyId)
        INSERT INTO CountryCurrencies2026 (CountryId, CurrencyId) VALUES (@CountryId, @CurrencyId);
END
GO

-- CountryCurrencies: get by country
CREATE PROCEDURE sp_CountryCurrencies2026_GetByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.Id AS CurrencyId, c.CurrencyCode, c.CurrencyName, c.CurrencySymbol
    FROM CountryCurrencies2026 cc
    JOIN Currencies2026 c ON cc.CurrencyId = c.Id
    WHERE cc.CountryId = @CountryId;
END
GO

-- CountryCurrencies: delete by country
CREATE PROCEDURE sp_CountryCurrencies2026_DeleteByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM CountryCurrencies2026 WHERE CountryId = @CountryId;
END
GO

-- CountryBorders: insert by border cca3 (resolve id), enforce CountryId < BorderCountryId to avoid duplicates
CREATE PROCEDURE sp_CountryBorders2026_InsertByBorderCca3
    @CountryId INT,
    @BorderCca3 NCHAR(3)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @BorderId INT;
    SELECT @BorderId = Id FROM Countries2026 WHERE Cca3 = @BorderCca3;

    IF @BorderId IS NULL RETURN;

    DECLARE @A INT = @CountryId, @B INT = @BorderId;
    IF @A = @B RETURN;

    IF @A > @B
    BEGIN
        DECLARE @tmp INT = @A; SET @A = @B; SET @B = @tmp;
    END

    IF NOT EXISTS (SELECT 1 FROM CountryBorders2026 WHERE CountryId = @A AND BorderCountryId = @B)
        INSERT INTO CountryBorders2026 (CountryId, BorderCountryId) VALUES (@A, @B);
END
GO

-- CountryBorders: get by country (returns the other side(s))
CREATE PROCEDURE sp_CountryBorders2026_GetByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- return all borders that include CountryId either as CountryId or BorderCountryId
    SELECT
        CASE WHEN cb.CountryId = @CountryId THEN cb.BorderCountryId ELSE cb.CountryId END AS BorderCountryId,
        c.Cca3, c.CommonName
    FROM CountryBorders2026 cb
    JOIN Countries2026 c ON c.Id = CASE WHEN cb.CountryId = @CountryId THEN cb.BorderCountryId ELSE cb.CountryId END
    WHERE cb.CountryId = @CountryId OR cb.BorderCountryId = @CountryId;
END
GO

-- CountryBorders: delete by country (remove any border pairs involving country)
CREATE PROCEDURE sp_CountryBorders2026_DeleteByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM CountryBorders2026 WHERE CountryId = @CountryId OR BorderCountryId = @CountryId;
END
GO

-- Timezones: insert
CREATE PROCEDURE sp_CountryTimezones2026_Insert
    @CountryId INT,
    @Timezone NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM CountryTimezones2026 WHERE CountryId = @CountryId AND Timezone = @Timezone)
        INSERT INTO CountryTimezones2026 (CountryId, Timezone) VALUES (@CountryId, @Timezone);
END
GO

-- Timezones: get by country
CREATE PROCEDURE sp_CountryTimezones2026_GetByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Timezone FROM CountryTimezones2026 WHERE CountryId = @CountryId;
END
GO

-- Timezones: delete by country
CREATE PROCEDURE sp_CountryTimezones2026_DeleteByCountryId
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM CountryTimezones2026 WHERE CountryId = @CountryId;
END
GO