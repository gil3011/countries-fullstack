-- =========================================================
-- Quizzes Stored Procedures
-- =========================================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==========================================
-- CREATE
-- ==========================================

-- 1. Create a new Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_CreateQuiz
    @Title NVARCHAR(255),
    @CreatorId INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO FP_Quizzes2026 (Title, CreatorId, IsPublic, Likes)
    VALUES (@Title, @CreatorId, 0, 0); -- Always starts private with 0 likes
    
    SELECT SCOPE_IDENTITY() AS NewQuizId;
END
GO

-- 2. Add Question to Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_AddQuestion
    @QuizId INT,
    @UserId INT,  -- ADDED FOR OWNERSHIP CHECK
    @Text NVARCHAR(MAX),
    @OptionA NVARCHAR(1000),
    @OptionB NVARCHAR(1000),
    @OptionC NVARCHAR(1000),
    @OptionD NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if quiz is owned by user
    IF NOT EXISTS(SELECT 1 FROM FP_Quizzes2026 WHERE Id = @QuizId AND CreatorId = @UserId)
    BEGIN
        SELECT 0 AS NewQuestionId; -- Cannot add to public or unowned quiz
        RETURN;
    END

    INSERT INTO FP_Questions2026 (QuizId, [Text], OptionA, OptionB, OptionC, OptionD)
    VALUES (@QuizId, @Text, @OptionA, @OptionB, @OptionC, @OptionD);
    
    SELECT SCOPE_IDENTITY() AS NewQuestionId;
END
GO

-- 3. Create Attempt
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_CreateAttempt
    @QuizId INT,
    @UserId INT,
    @Score INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO FP_QuizAttempts2026 (QuizId, UserId, Score, DateTaken)
    VALUES (@QuizId, @UserId, @Score, GETDATE());
    
    SELECT SCOPE_IDENTITY() AS NewAttemptId;
END
GO

-- 4. Add Attempt Answer
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_AddAttemptAnswer
    @AttemptId INT,
    @QuestionId INT,
    @UserAnswer NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO FP_QuizAttemptAnswers2026 (AttemptId, QuestionId, UserAnswer)
    VALUES (@AttemptId, @QuestionId, @UserAnswer);
    
    SELECT 1 AS Result;
END
GO

-- ==========================================
-- GET
-- ==========================================

-- 5. Get Quiz details
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetQuizById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, CreatorId, IsPublic, Likes
    FROM FP_Quizzes2026
    WHERE Id = @Id;
END
GO

-- 6. Get Questions by Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetQuestionsByQuizId
    @QuizId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, QuizId, [Text], OptionA, OptionB, OptionC, OptionD
    FROM FP_Questions2026
    WHERE QuizId = @QuizId;
END
GO

-- 7. Get Attempt Details
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetAttempt
    @AttemptId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.Id, a.QuizId, a.UserId, a.Score, a.DateTaken, q.Title AS QuizTitle
    FROM FP_QuizAttempts2026 a
    INNER JOIN FP_Quizzes2026 q ON a.QuizId = q.Id
    WHERE a.Id = @AttemptId;
END
GO

-- 8. Get Attempt Answers
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetAttemptAnswers
    @AttemptId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AttemptId, QuestionId, UserAnswer
    FROM FP_QuizAttemptAnswers2026
    WHERE AttemptId = @AttemptId;
END
GO

-- 9. (Added) Get All Public Quizzes
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetAllPublicQuizzes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        q.Id, q.Title, q.CreatorId, q.IsPublic, q.Likes,
        (SELECT COUNT(*) FROM FP_Questions2026 WHERE QuizId = q.Id) AS QuestionCount
    FROM FP_Quizzes2026 q
    WHERE q.IsPublic = 1;
END
GO

-- 10. (Added) Get Quizzes by Creator
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetQuizzesByUserId
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        q.Id, q.Title, q.CreatorId, q.IsPublic, q.Likes,
        (SELECT COUNT(*) FROM FP_Questions2026 WHERE QuizId = q.Id) AS QuestionCount
    FROM FP_Quizzes2026 q
    WHERE q.CreatorId = @UserId;
END
GO

-- 11. (Added) Get Attempts by User Id
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetAttemptsByUserId
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.Id, a.QuizId, a.UserId, a.Score, a.DateTaken, q.Title AS QuizTitle
    FROM FP_QuizAttempts2026 a
    INNER JOIN FP_Quizzes2026 q ON a.QuizId = q.Id
    WHERE a.UserId = @UserId;
END
GO

-- ==========================================
-- UPDATE
-- ==========================================

-- 11. Update Quiz (Only if private and owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_UpdateQuiz
    @Id INT,
    @UserId INT, -- ADDED FOR OWNERSHIP CHECK
    @Title NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE FP_Quizzes2026
    SET Title = @Title
    WHERE Id = @Id AND CreatorId = @UserId;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 12. Update Question (Only if quiz is private and owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_UpdateQuestion
    @Id INT,
    @UserId INT, -- ADDED FOR OWNERSHIP CHECK
    @Text NVARCHAR(MAX),
    @OptionA NVARCHAR(1000),
    @OptionB NVARCHAR(1000),
    @OptionC NVARCHAR(1000),
    @OptionD NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @QuizId INT;
    SELECT @QuizId = QuizId FROM FP_Questions2026 WHERE Id = @Id;

    IF NOT EXISTS(SELECT 1 FROM FP_Quizzes2026 WHERE Id = @QuizId AND CreatorId = @UserId)
    BEGIN
        SELECT 0 AS Result; -- Failed, quiz is public or unowned
        RETURN;
    END

    UPDATE FP_Questions2026
    SET [Text] = @Text, OptionA = @OptionA, OptionB = @OptionB, OptionC = @OptionC, OptionD = @OptionD
    WHERE Id = @Id;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 13. Publish Quiz (Only if owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_PublishQuiz
    @Id INT,
    @UserId INT -- ADDED FOR OWNERSHIP CHECK
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE FP_Quizzes2026
    SET IsPublic = 1
    WHERE Id = @Id AND CreatorId = @UserId AND IsPublic = 0;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 13b. Toggle Like to Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_ToggleLike
    @QuizId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM FP_Quizzes2026 WHERE Id = @QuizId AND IsPublic = 1)
    BEGIN
        SELECT 0 AS Result; -- Quiz not found or not public
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM FP_QuizLikes2026 WHERE QuizId = @QuizId AND UserId = @UserId)
    BEGIN
        -- Unlike
        DELETE FROM FP_QuizLikes2026 WHERE QuizId = @QuizId AND UserId = @UserId;
        UPDATE FP_Quizzes2026 SET Likes = Likes - 1 WHERE Id = @QuizId;
        SELECT 2 AS Result; -- 2 means unliked
    END
    ELSE
    BEGIN
        -- Like
        INSERT INTO FP_QuizLikes2026 (QuizId, UserId) VALUES (@QuizId, @UserId);
        UPDATE FP_Quizzes2026 SET Likes = Likes + 1 WHERE Id = @QuizId;
        SELECT 1 AS Result; -- 1 means liked
    END
END
GO

-- ==========================================
-- DELETE
-- ==========================================

-- 14. Delete Quiz (Only if private and owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_DeleteQuiz
    @Id INT,
    @UserId INT -- ADDED FOR OWNERSHIP CHECK
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM FP_Quizzes2026 WHERE Id = @Id AND CreatorId = @UserId;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 15. Delete Question (Only if quiz is private and owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_DeleteQuestion
    @Id INT,
    @UserId INT -- ADDED FOR OWNERSHIP CHECK
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @QuizId INT;
    SELECT @QuizId = QuizId FROM FP_Questions2026 WHERE Id = @Id;

    IF NOT EXISTS(SELECT 1 FROM FP_Quizzes2026 WHERE Id = @QuizId AND CreatorId = @UserId)
    BEGIN
        SELECT 0 AS Result; -- Failed
        RETURN;
    END

    DELETE FROM FP_Questions2026 WHERE Id = @Id;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 16. Delete Attempt (Assuming user can delete their own attempt, or maybe admin. Added UserId check for safety)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_DeleteAttempt
    @AttemptId INT,
    @UserId INT -- ADDED FOR OWNERSHIP CHECK
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM FP_QuizAttempts2026 WHERE Id = @AttemptId AND UserId = @UserId;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 17. Unpublish Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_UnpublishQuiz
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE FP_Quizzes2026
    SET IsPublic = 0
    WHERE Id = @Id AND CreatorId = @UserId AND IsPublic = 1;
END
GO

-- 11. (Added) Get Attempts by User Id
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetAttemptsByUserId
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.Id, a.QuizId, a.UserId, a.Score, a.DateTaken, q.Title AS QuizTitle
    FROM FP_QuizAttempts2026 a
    INNER JOIN FP_Quizzes2026 q ON a.QuizId = q.Id
    WHERE a.UserId = @UserId;
END
GO

-- ==========================================
-- UPDATE
-- ==========================================

-- 11. Update Quiz (Only if private and owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_UpdateQuiz
    @Id INT,
    @UserId INT, -- ADDED FOR OWNERSHIP CHECK
    @Title NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE FP_Quizzes2026
    SET Title = @Title
    WHERE Id = @Id AND CreatorId = @UserId;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 12. Update Question (Only if quiz is private and owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_UpdateQuestion
    @Id INT,
    @UserId INT, -- ADDED FOR OWNERSHIP CHECK
    @Text NVARCHAR(MAX),
    @OptionA NVARCHAR(1000),
    @OptionB NVARCHAR(1000),
    @OptionC NVARCHAR(1000),
    @OptionD NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @QuizId INT;
    SELECT @QuizId = QuizId FROM FP_Questions2026 WHERE Id = @Id;

    IF NOT EXISTS(SELECT 1 FROM FP_Quizzes2026 WHERE Id = @QuizId AND CreatorId = @UserId)
    BEGIN
        SELECT 0 AS Result; -- Failed, quiz is public or unowned
        RETURN;
    END

    UPDATE FP_Questions2026
    SET [Text] = @Text, OptionA = @OptionA, OptionB = @OptionB, OptionC = @OptionC, OptionD = @OptionD
    WHERE Id = @Id;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 13. Publish Quiz (Only if owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_PublishQuiz
    @Id INT,
    @UserId INT -- ADDED FOR OWNERSHIP CHECK
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE FP_Quizzes2026
    SET IsPublic = 1
    WHERE Id = @Id AND CreatorId = @UserId AND IsPublic = 0;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 13b. Toggle Like to Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_ToggleLike
    @QuizId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM FP_Quizzes2026 WHERE Id = @QuizId AND IsPublic = 1)
    BEGIN
        SELECT 0 AS Result; -- Quiz not found or not public
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM FP_QuizLikes2026 WHERE QuizId = @QuizId AND UserId = @UserId)
    BEGIN
        -- Unlike
        DELETE FROM FP_QuizLikes2026 WHERE QuizId = @QuizId AND UserId = @UserId;
        UPDATE FP_Quizzes2026 SET Likes = Likes - 1 WHERE Id = @QuizId;
        SELECT 2 AS Result; -- 2 means unliked
    END
    ELSE
    BEGIN
        -- Like
        INSERT INTO FP_QuizLikes2026 (QuizId, UserId) VALUES (@QuizId, @UserId);
        UPDATE FP_Quizzes2026 SET Likes = Likes + 1 WHERE Id = @QuizId;
        SELECT 1 AS Result; -- 1 means liked
    END
END
GO

-- ==========================================
-- DELETE
-- ==========================================

-- 14. Delete Quiz (Only if private and owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_DeleteQuiz
    @Id INT,
    @UserId INT -- ADDED FOR OWNERSHIP CHECK
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM FP_Quizzes2026 WHERE Id = @Id AND CreatorId = @UserId;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 15. Delete Question (Only if quiz is private and owned by user)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_DeleteQuestion
    @Id INT,
    @UserId INT -- ADDED FOR OWNERSHIP CHECK
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @QuizId INT;
    SELECT @QuizId = QuizId FROM FP_Questions2026 WHERE Id = @Id;

    IF NOT EXISTS(SELECT 1 FROM FP_Quizzes2026 WHERE Id = @QuizId AND CreatorId = @UserId)
    BEGIN
        SELECT 0 AS Result; -- Failed
        RETURN;
    END

    DELETE FROM FP_Questions2026 WHERE Id = @Id;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 16. Delete Attempt (Assuming user can delete their own attempt, or maybe admin. Added UserId check for safety)
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_DeleteAttempt
    @AttemptId INT,
    @UserId INT -- ADDED FOR OWNERSHIP CHECK
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM FP_QuizAttempts2026 WHERE Id = @AttemptId AND UserId = @UserId;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 17. Unpublish Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_UnpublishQuiz
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE FP_Quizzes2026
    SET IsPublic = 0
    WHERE Id = @Id AND CreatorId = @UserId AND IsPublic = 1;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- ==========================================
-- QUIZ COUNTRIES
-- ==========================================

-- 18. Add Country to Quiz
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

-- 19. Clear all Countries from Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_ClearCountries
    @QuizId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM FP_QuizCountries2026 WHERE QuizId = @QuizId;
    SELECT 1 AS Result;
END
GO

-- 20. Get all Countries for a Quiz
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_GetQuizCountries
    @QuizId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CountryId FROM FP_QuizCountries2026 WHERE QuizId = @QuizId;
END
GO
