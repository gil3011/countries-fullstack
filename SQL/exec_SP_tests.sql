-- ============================================================================
-- SQL SERVER STORED PROCEDURE TEST SUITE (ALL-IN-ONE)
-- ============================================================================
-- This script contains precise, executable commands to test all tables and 
-- stored procedures created for the 2026 Countries Database schema.
-- It executes in sequence: Setup -> Inserts -> Reads -> Updates -> Deletes.
-- ============================================================================

SET NOCOUNT OFF;

PRINT '============================================================================';
PRINT ' STEP 1: INITIALIZE TEST VARIABLES & INSERT MOCK CORES';
PRINT '============================================================================';

-- Declarations for testing scope
DECLARE @CanadaId INT;
DECLARE @USAId INT;
DECLARE @LangEnId INT;
DECLARE @LangFrId INT;
DECLARE @CadId INT;
DECLARE @UsdId INT;

--------------------------------------------------------------------------------
-- 1. Test sp_Countries2026_Insert
--------------------------------------------------------------------------------
PRINT '--> Testing: sp_Countries2026_Insert (Canada)';
EXEC sp_Countries2026_Insert 
    @Cca3 = 'CAN', 
    @CommonName = 'Canada', 
    @OfficialName = 'Canada', 
    @Region = 2, 
    @Subregion = 'North America', 
    @Latitude = 60.0, 
    @Longitude = -95.0, 
    @AreaKm2 = 9984670, 
    @IsLandlocked = 0, 
    @Population = 38000000, 
    @FlagUrl = 'https://example.com/flags/can.png', 
    @WikipediaUrl = 'https://en.wikipedia.org/wiki/Canada',
    @NewId = @CanadaId OUTPUT;

PRINT '   SUCCESS: Inserted Canada with Generated ID: ' + CAST(@CanadaId AS VARCHAR(10));

PRINT '--> Testing: sp_Countries2026_Insert (United States)';
EXEC sp_Countries2026_Insert 
    @Cca3 = 'USA', 
    @CommonName = 'United States', 
    @OfficialName = 'United States of America', 
    @Region = 2, 
    @Subregion = 'North America', 
    @Latitude = 38.0, 
    @Longitude = -97.0, 
    @AreaKm2 = 9833520, 
    @IsLandlocked = 0, 
    @Population = 331000000, 
    @FlagUrl = 'https://example.com/flags/usa.png', 
    @WikipediaUrl = 'https://en.wikipedia.org/wiki/United_States',
    @NewId = @USAId OUTPUT;

PRINT '   SUCCESS: Inserted USA with Generated ID: ' + CAST(@USAId AS VARCHAR(10));

--------------------------------------------------------------------------------
-- 2. Test sp_Capitals2026_Insert
--------------------------------------------------------------------------------
PRINT '--> Testing: sp_Capitals2026_Insert';
EXEC sp_Capitals2026_Insert @CountryId = @CanadaId, @Name = 'Ottawa', @Latitude = 45.4215, @Longitude = -75.6972;
EXEC sp_Capitals2026_Insert @CountryId = @USAId, @Name = 'Washington D.C.', @Latitude = 38.9072, @Longitude = -77.0369;
PRINT '   SUCCESS: Capitals mapped to Country IDs.';

--------------------------------------------------------------------------------
-- 3. Test sp_Languages2026_GetOrCreate
--------------------------------------------------------------------------------
PRINT '--> Testing: sp_Languages2026_GetOrCreate';
EXEC sp_Languages2026_GetOrCreate @Iso639_1 = 'en', @LanguageName = 'English', @LanguageId = @LangEnId OUTPUT;
EXEC sp_Languages2026_GetOrCreate @Iso639_1 = 'fr', @LanguageName = 'French', @LanguageId = @LangFrId OUTPUT;
PRINT '   SUCCESS: Languages structured (EN ID: ' + CAST(@LangEnId AS VARCHAR(10)) + ', FR ID: ' + CAST(@LangFrId AS VARCHAR(10)) + ')';

--------------------------------------------------------------------------------
-- 4. Test sp_CountryLanguages2026_Insert
--------------------------------------------------------------------------------
PRINT '--> Testing: sp_CountryLanguages2026_Insert';
EXEC sp_CountryLanguages2026_Insert @CountryId = @CanadaId, @LanguageId = @LangEnId;
EXEC sp_CountryLanguages2026_Insert @CountryId = @CanadaId, @LanguageId = @LangFrId;
EXEC sp_CountryLanguages2026_Insert @CountryId = @USAId, @LanguageId = @LangEnId;
PRINT '   SUCCESS: Country to Language assignment completed.';

--------------------------------------------------------------------------------
-- 5. Test sp_Currencies2026_GetOrCreate
--------------------------------------------------------------------------------
PRINT '--> Testing: sp_Currencies2026_GetOrCreate';
EXEC sp_Currencies2026_GetOrCreate @CurrencyCode = 'CAD', @CurrencyName = 'Canadian Dollar', @CurrencySymbol = '$', @CurrencyId = @CadId OUTPUT;
EXEC sp_Currencies2026_GetOrCreate @CurrencyCode = 'USD', @CurrencyName = 'US Dollar', @CurrencySymbol = '$', @CurrencyId = @UsdId OUTPUT;
PRINT '   SUCCESS: Currencies structured (CAD ID: ' + CAST(@CadId AS VARCHAR(10)) + ', USD ID: ' + CAST(@UsdId AS VARCHAR(10)) + ')';

--------------------------------------------------------------------------------
-- 6. Test sp_CountryCurrencies2026_Insert
--------------------------------------------------------------------------------
PRINT '--> Testing: sp_CountryCurrencies2026_Insert';
EXEC sp_CountryCurrencies2026_Insert @CountryId = @CanadaId, @CurrencyId = @CadId;
EXEC sp_CountryCurrencies2026_Insert @CountryId = @USAId, @CurrencyId = @UsdId;
PRINT '   SUCCESS: Country to Currency assignment completed.';

--------------------------------------------------------------------------------
-- 7. Test sp_CountryBorders2026_InsertByBorderCca3
--------------------------------------------------------------------------------
PRINT '--> Testing: sp_CountryBorders2026_InsertByBorderCca3';
EXEC sp_CountryBorders2026_InsertByBorderCca3 @CountryId = @CanadaId, @BorderCca3 = 'USA';
PRINT '   SUCCESS: Border relation mapped.';

--------------------------------------------------------------------------------
-- 8. Test sp_CountryTimezones2026_Insert
--------------------------------------------------------------------------------
PRINT '--> Testing: sp_CountryTimezones2026_Insert';
EXEC sp_CountryTimezones2026_Insert @CountryId = @CanadaId, @Timezone = 'UTC-05:00';
EXEC sp_CountryTimezones2026_Insert @CountryId = @CanadaId, @Timezone = 'UTC-04:00';
EXEC sp_CountryTimezones2026_Insert @CountryId = @USAId, @Timezone = 'UTC-05:00';
PRINT '   SUCCESS: Timezone listings updated.';

PRINT '';
PRINT '============================================================================';
PRINT ' STEP 2: VERIFY AND READ DATA SELECTIONS';
PRINT '============================================================================';

PRINT '--> Execution: sp_Countries2026_ReadAll';
EXEC sp_Countries2026_ReadAll;

PRINT '--> Execution: sp_Countries2026_GetByCca3 (USA)';
EXEC sp_Countries2026_GetByCca3 @Cca3 = 'USA';

PRINT '--> Execution: sp_Capitals2026_GetByCountryId (Canada)';
EXEC sp_Capitals2026_GetByCountryId @CountryId = @CanadaId;

PRINT '--> Execution: sp_CountryLanguages2026_GetByCountryId (Canada)';
EXEC sp_CountryLanguages2026_GetByCountryId @CountryId = @CanadaId;

PRINT '--> Execution: sp_CountryCurrencies2026_GetByCountryId (Canada)';
EXEC sp_CountryCurrencies2026_GetByCountryId @CountryId = @CanadaId;

PRINT '--> Execution: sp_CountryBorders2026_GetByCountryId (Canada)';
EXEC sp_CountryBorders2026_GetByCountryId @CountryId = @CanadaId;

PRINT '--> Execution: sp_CountryTimezones2026_GetByCountryId (Canada)';
EXEC sp_CountryTimezones2026_GetByCountryId @CountryId = @CanadaId;

PRINT '';
PRINT '============================================================================';
PRINT ' STEP 3: EXECUTE MUTATION DATA UPDATES';
PRINT '============================================================================';

PRINT '--> Testing: sp_Countries2026_Update (Updating Canada Population)';
EXEC sp_Countries2026_Update 
    @Id = @CanadaId, 
    @Cca3 = 'CAN', 
    @CommonName = 'Canada', 
    @OfficialName = 'Dominion of Canada', 
    @Region = 2, 
    @Subregion = 'North America', 
    @Latitude = 60.0, 
    @Longitude = -95.0, 
    @AreaKm2 = 9984670, 
    @IsLandlocked = 0, 
    @Population = 40000000, -- Modified field
    @FlagUrl = 'https://example.com/flags/can-new.png', 
    @WikipediaUrl = 'https://en.wikipedia.org/wiki/Canada';

PRINT '   Verifying update via Read Proc:';
EXEC sp_Countries2026_GetByCca3 @Cca3 = 'CAN';

PRINT '';
PRINT '============================================================================';
PRINT ' STEP 4: MUTATION TEARDOWN AND DATA CLEANUP';
PRINT '============================================================================';

PRINT '--> Testing explicit relational deletions for Canada...';
EXEC sp_Capitals2026_DeleteByCountryId @CountryId = @CanadaId;
EXEC sp_CountryLanguages2026_DeleteByCountryId @CountryId = @CanadaId;
EXEC sp_CountryCurrencies2026_DeleteByCountryId @CountryId = @CanadaId;
EXEC sp_CountryBorders2026_DeleteByCountryId @CountryId = @CanadaId;
EXEC sp_CountryTimezones2026_DeleteByCountryId @CountryId = @CanadaId;
PRINT '   SUCCESS: Canada isolated relational child fields purged.';

PRINT '--> Testing sp_Countries2026_Delete (Purging Core Country Entities)...';
EXEC sp_Countries2026_Delete @Id = @CanadaId;
EXEC sp_Countries2026_Delete @Id = @USAId;
PRINT '   SUCCESS: Canada and USA removed. (USA child elements dropped automatically via CASCADE).';

-- Final dictionary cleanup to return environment state cleanly
DELETE FROM Languages2026 WHERE Iso639_1 IN ('en', 'fr');
DELETE FROM Currencies2026 WHERE CurrencyCode IN ('CAD', 'USD');

PRINT '   SUCCESS: Shared dictionaries cleared.';
PRINT '============================================================================';
PRINT ' TEST SUITE COMPLETED: ENVIRONMENT RETURNDED TO ORIGINAL STATE';
PRINT '============================================================================';