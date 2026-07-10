SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. Add Country To Wishlist
CREATE PROCEDURE FP_sp_UserCountries_AddCountryToWishlist
    @UserId INT,
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM FP_UserCountries2026 WHERE UserId = @UserId AND CountryId = @CountryId)
    BEGIN
        UPDATE FP_UserCountries2026
        SET IsWishlist = 1
        WHERE UserId = @UserId AND CountryId = @CountryId;
    END
    ELSE
    BEGIN
        INSERT INTO FP_UserCountries2026 (UserId, CountryId, IsWishlist, IsVisited)
        VALUES (@UserId, @CountryId, 1, 0);
    END
    
    SELECT 1 AS Result;
END
GO

-- 2. Remove Country From Wishlist
CREATE PROCEDURE FP_sp_UserCountries_RemoveCountryFromWishlist
    @UserId INT,
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- If it's the last flag standing (IsVisited is 0), we delete. Otherwise, we just set IsWishlist to 0.
    IF EXISTS (SELECT 1 FROM FP_UserCountries2026 WHERE UserId = @UserId AND CountryId = @CountryId AND IsVisited = 1)
    BEGIN
        UPDATE FP_UserCountries2026
        SET IsWishlist = 0
        WHERE UserId = @UserId AND CountryId = @CountryId;
    END
    ELSE
    BEGIN
        DELETE FROM FP_UserCountries2026
        WHERE UserId = @UserId AND CountryId = @CountryId;
    END
    
    SELECT 1 AS Result;
END
GO

-- 3. Get Wishlist Countries
CREATE PROCEDURE FP_sp_UserCountries_GetWishlist
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT c.Id, c.Cca3, c.CommonName, c.OfficialName, c.Region, c.Subregion,
           c.Latitude, c.Longitude, c.AreaKm2, c.IsLandlocked, c.[Population],
           c.FlagUrl, c.WikipediaUrl
    FROM FP_Countries2026 c
    INNER JOIN FP_UserCountries2026 uc ON c.Id = uc.CountryId
    WHERE uc.UserId = @UserId AND uc.IsWishlist = 1;
END
GO

-- 4. Add Country To Visited
CREATE PROCEDURE FP_sp_UserCountries_AddCountryToVisited
    @UserId INT,
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM FP_UserCountries2026 WHERE UserId = @UserId AND CountryId = @CountryId)
    BEGIN
        UPDATE FP_UserCountries2026
        SET IsVisited = 1
        WHERE UserId = @UserId AND CountryId = @CountryId;
    END
    ELSE
    BEGIN
        INSERT INTO FP_UserCountries2026 (UserId, CountryId, IsWishlist, IsVisited)
        VALUES (@UserId, @CountryId, 0, 1);
    END
    
    SELECT 1 AS Result;
END
GO

-- 5. Remove Country From Visited
CREATE PROCEDURE FP_sp_UserCountries_RemoveCountryFromVisited
    @UserId INT,
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- If it's the last flag standing (IsWishlist is 0), we delete. Otherwise, we just set IsVisited to 0.
    IF EXISTS (SELECT 1 FROM FP_UserCountries2026 WHERE UserId = @UserId AND CountryId = @CountryId AND IsWishlist = 1)
    BEGIN
        UPDATE FP_UserCountries2026
        SET IsVisited = 0
        WHERE UserId = @UserId AND CountryId = @CountryId;
    END
    ELSE
    BEGIN
        DELETE FROM FP_UserCountries2026
        WHERE UserId = @UserId AND CountryId = @CountryId;
    END
    
    SELECT 1 AS Result;
END
GO

-- 6. Get Visited Countries
CREATE PROCEDURE FP_sp_UserCountries_GetVisited
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT c.Id, c.Cca3, c.CommonName, c.OfficialName, c.Region, c.Subregion,
           c.Latitude, c.Longitude, c.AreaKm2, c.IsLandlocked, c.[Population],
           c.FlagUrl, c.WikipediaUrl
    FROM FP_Countries2026 c
    INNER JOIN FP_UserCountries2026 uc ON c.Id = uc.CountryId
    WHERE uc.UserId = @UserId AND uc.IsVisited = 1;
END
GO
