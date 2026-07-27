-- Disable foreign key constraints temporarily to ensure a smooth drop execution
SET NOCOUNT ON;

PRINT '========================================';
PRINT 'DROPPING ALL 2026 COUNTRIES TABLES';
PRINT '========================================';

-- 1. Drop Junction / Many-to-Many / Child Tables First
IF OBJECT_ID('dbo.CountryTimezones2026', 'U') IS NOT NULL 
    DROP TABLE CountryTimezones2026;

IF OBJECT_ID('dbo.CountryBorders2026', 'U') IS NOT NULL 
    DROP TABLE CountryBorders2026;

IF OBJECT_ID('dbo.CountryCurrencies2026', 'U') IS NOT NULL 
    DROP TABLE CountryCurrencies2026;

IF OBJECT_ID('dbo.CountryLanguages2026', 'U') IS NOT NULL 
    DROP TABLE CountryLanguages2026;

IF OBJECT_ID('dbo.Capitals2026', 'U') IS NOT NULL 
    DROP TABLE Capitals2026;

-- 2. Drop Parent / Dictionary Tables Last
IF OBJECT_ID('dbo.Currencies2026', 'U') IS NOT NULL 
    DROP TABLE Currencies2026;

IF OBJECT_ID('dbo.Languages2026', 'U') IS NOT NULL 
    DROP TABLE Languages2026;

IF OBJECT_ID('dbo.Countries2026', 'U') IS NOT NULL 
    DROP TABLE Countries2026;

PRINT '========================================';
PRINT 'ALL TABLES DROPPED SUCCESSFULLY';
PRINT '========================================';