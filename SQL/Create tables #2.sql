CREATE TABLE FP_UserCountries2026 (
    UserId INT NOT NULL,
    CountryId INT NOT NULL,
    IsWishlist BIT NOT NULL DEFAULT 0,
    IsVisited BIT NOT NULL DEFAULT 0,
    PRIMARY KEY (UserId, CountryId),
    CONSTRAINT CHK_WishlistOrVisited CHECK (IsWishlist = 1 OR IsVisited = 1),
    CONSTRAINT FK_UserCountries_Users FOREIGN KEY (UserId) REFERENCES FP_Users2026(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserCountries_Countries FOREIGN KEY (CountryId) REFERENCES FP_Countries2026(Id) ON DELETE CASCADE
);