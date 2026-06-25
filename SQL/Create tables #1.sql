-- Countries and related tables (SQL Server)

CREATE TABLE Countries2026 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Cca3 NCHAR(3) NOT NULL UNIQUE,            -- alpha-3 code, e.g. ISR
    CommonName NVARCHAR(200) NOT NULL,
    OfficialName NVARCHAR(300) NOT NULL,
    Region INT NOT NULL,                       -- store Constants.Region as integer
    Subregion NVARCHAR(150) NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    AreaKm2 FLOAT NULL,
    IsLandlocked BIT NOT NULL DEFAULT 0,
    [Population] INT NOT NULL DEFAULT 0,
    FlagUrl NVARCHAR(1000) NULL,
    WikipediaUrl NVARCHAR(1000) NULL
);

CREATE TABLE Capitals2026 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CountryId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    CONSTRAINT FK_Capitals_Countries FOREIGN KEY (CountryId) REFERENCES Countries2026(Id) ON DELETE CASCADE
);

CREATE TABLE Languages2026 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Iso639_1 NCHAR(2) NOT NULL UNIQUE,        -- e.g. en, fr
    LanguageName NVARCHAR(200) NOT NULL
);

CREATE TABLE Currencies2026 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CurrencyCode NCHAR(3) NOT NULL UNIQUE,    -- ISO 4217 e.g. USD
    CurrencyName NVARCHAR(200) NOT NULL,
    CurrencySymbol NVARCHAR(50) NULL
);

-- Many-to-many: Country <-> Language
CREATE TABLE CountryLanguages2026 (
    CountryId INT NOT NULL,
    LanguageId INT NOT NULL,
    PRIMARY KEY (CountryId, LanguageId),
    CONSTRAINT FK_CountryLanguages_Countries FOREIGN KEY (CountryId) REFERENCES Countries2026(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CountryLanguages_Languages FOREIGN KEY (LanguageId) REFERENCES Languages2026(Id) ON DELETE CASCADE
);

-- Many-to-many: Country <-> Currency
CREATE TABLE CountryCurrencies2026 (
    CountryId INT NOT NULL,
    CurrencyId INT NOT NULL,
    PRIMARY KEY (CountryId, CurrencyId),
    CONSTRAINT FK_CountryCurrencies_Countries FOREIGN KEY (CountryId) REFERENCES Countries2026(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CountryCurrencies_Currencies FOREIGN KEY (CurrencyId) REFERENCES Currencies2026(Id) ON DELETE CASCADE
);

-- Self-referencing many-to-many for borders.
-- To avoid duplicate undirected entries (A-B and B-A), enforce CountryId < BorderCountryId when inserting.
CREATE TABLE CountryBorders2026 (
    CountryId INT NOT NULL,
    BorderCountryId INT NOT NULL,
    PRIMARY KEY (CountryId, BorderCountryId),
    CONSTRAINT FK_CountryBorders_Country FOREIGN KEY (CountryId) REFERENCES Countries2026(Id),
    CONSTRAINT FK_CountryBorders_BorderCountry FOREIGN KEY (BorderCountryId) REFERENCES Countries2026(Id)
);

-- Country timezones (simple list of strings)
CREATE TABLE CountryTimezones2026 (
    CountryId INT NOT NULL,
    Timezone NVARCHAR(100) NOT NULL,
    PRIMARY KEY (CountryId, Timezone),
    CONSTRAINT FK_CountryTimezones_Countries FOREIGN KEY (CountryId) REFERENCES Countries2026(Id) ON DELETE CASCADE
);