-- Rename Tables
EXEC sp_rename 'Countries2026', 'FP_Countries2026';
EXEC sp_rename 'Capitals2026', 'FP_Capitals2026';
EXEC sp_rename 'Languages2026', 'FP_Languages2026';
EXEC sp_rename 'Currencies2026', 'FP_Currencies2026';
EXEC sp_rename 'CountryLanguages2026', 'FP_CountryLanguages2026';
EXEC sp_rename 'CountryCurrencies2026', 'FP_CountryCurrencies2026';
EXEC sp_rename 'CountryBorders2026', 'FP_CountryBorders2026';
EXEC sp_rename 'CountryTimezones2026', 'FP_CountryTimezones2026';

-- Rename Stored Procedures
EXEC sp_rename 'sp_Countries2026_ReadAll', 'FP_sp_Countries2026_ReadAll';
EXEC sp_rename 'sp_Countries2026_GetByCca3', 'FP_sp_Countries2026_GetByCca3';
EXEC sp_rename 'sp_Countries2026_Insert', 'FP_sp_Countries2026_Insert';
EXEC sp_rename 'sp_Countries2026_Update', 'FP_sp_Countries2026_Update';
EXEC sp_rename 'sp_Countries2026_Delete', 'FP_sp_Countries2026_Delete';

EXEC sp_rename 'sp_Capitals2026_Insert', 'FP_sp_Capitals2026_Insert';
EXEC sp_rename 'sp_Capitals2026_GetByCountryId', 'FP_sp_Capitals2026_GetByCountryId';
EXEC sp_rename 'sp_Capitals2026_DeleteByCountryId', 'FP_sp_Capitals2026_DeleteByCountryId';

EXEC sp_rename 'sp_Languages2026_GetOrCreate', 'FP_sp_Languages2026_GetOrCreate';

EXEC sp_rename 'sp_CountryLanguages2026_Insert', 'FP_sp_CountryLanguages2026_Insert';
EXEC sp_rename 'sp_CountryLanguages2026_GetByCountryId', 'FP_sp_CountryLanguages2026_GetByCountryId';
EXEC sp_rename 'sp_CountryLanguages2026_DeleteByCountryId', 'FP_sp_CountryLanguages2026_DeleteByCountryId';

EXEC sp_rename 'sp_Currencies2026_GetOrCreate', 'FP_sp_Currencies2026_GetOrCreate';

EXEC sp_rename 'sp_CountryCurrencies2026_Insert', 'FP_sp_CountryCurrencies2026_Insert';
EXEC sp_rename 'sp_CountryCurrencies2026_GetByCountryId', 'FP_sp_CountryCurrencies2026_GetByCountryId';
EXEC sp_rename 'sp_CountryCurrencies2026_DeleteByCountryId', 'FP_sp_CountryCurrencies2026_DeleteByCountryId';

EXEC sp_rename 'sp_CountryBorders2026_InsertByBorderCca3', 'FP_sp_CountryBorders2026_InsertByBorderCca3';
EXEC sp_rename 'sp_CountryBorders2026_GetByCountryId', 'FP_sp_CountryBorders2026_GetByCountryId';
EXEC sp_rename 'sp_CountryBorders2026_DeleteByCountryId', 'FP_sp_CountryBorders2026_DeleteByCountryId';

EXEC sp_rename 'sp_CountryTimezones2026_Insert', 'FP_sp_CountryTimezones2026_Insert';
EXEC sp_rename 'sp_CountryTimezones2026_GetByCountryId', 'FP_sp_CountryTimezones2026_GetByCountryId';
EXEC sp_rename 'sp_CountryTimezones2026_DeleteByCountryId', 'FP_sp_CountryTimezones2026_DeleteByCountryId';
GO
