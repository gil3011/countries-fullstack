-- =========================================================
-- Create QuizCountries Junction Table & Stored Procedures
-- Run this script in your SQL environment to apply the schema updates
-- =========================================================

-- 1. Create the new table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FP_QuizCountries2026' and xtype='U')
BEGIN
    CREATE TABLE FP_QuizCountries2026 (
        QuizId INT NOT NULL,
        CountryId INT NOT NULL,
        PRIMARY KEY (QuizId, CountryId),
        CONSTRAINT FK_QuizCountries_Quizzes FOREIGN KEY (QuizId) REFERENCES FP_Quizzes2026(Id) ON DELETE CASCADE,
        CONSTRAINT FK_QuizCountries_Countries FOREIGN KEY (CountryId) REFERENCES FP_Countries2026(Id) ON DELETE CASCADE
    );
END
GO

-- 2. Add Country to Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_AddCountry
    @QuizId INT,
    @CountryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Insert only if it doesn't already exist to prevent duplicate key errors
    IF NOT EXISTS(SELECT 1 FROM FP_QuizCountries2026 WHERE QuizId = @QuizId AND CountryId = @CountryId)
    BEGIN
        INSERT INTO FP_QuizCountries2026 (QuizId, CountryId)
        VALUES (@QuizId, @CountryId);
    END
    
    SELECT 1 AS Result;
END
GO

-- 3. Clear all Countries from Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_ClearCountries
    @QuizId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM FP_QuizCountries2026 WHERE QuizId = @QuizId;
    SELECT 1 AS Result;
END
GO

-- 4. Get all Countries for a Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetQuizCountries
    @QuizId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CountryId FROM FP_QuizCountries2026 WHERE QuizId = @QuizId;
END
GO
