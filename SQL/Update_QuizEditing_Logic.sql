-- 1. Allow updating quiz regardless of IsPublic
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_UpdateQuiz
    @Id INT,
    @UserId INT,
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

-- 2. Allow adding questions regardless of IsPublic
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_AddQuestion
    @QuizId INT,
    @UserId INT,
    @Text NVARCHAR(MAX),
    @OptionA NVARCHAR(1000),
    @OptionB NVARCHAR(1000),
    @OptionC NVARCHAR(1000),
    @OptionD NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS(SELECT 1 FROM FP_Quizzes2026 WHERE Id = @QuizId AND CreatorId = @UserId)
    BEGIN
        SELECT 0 AS NewQuestionId;
        RETURN;
    END

    INSERT INTO FP_Questions2026 (QuizId, [Text], OptionA, OptionB, OptionC, OptionD)
    VALUES (@QuizId, @Text, @OptionA, @OptionB, @OptionC, @OptionD);
    
    SELECT SCOPE_IDENTITY() AS NewQuestionId;
END
GO

-- 3. Allow updating questions regardless of IsPublic
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_UpdateQuestion
    @Id INT,
    @UserId INT,
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
        SELECT 0 AS Result;
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

-- 4. Allow deleting questions regardless of IsPublic
CREATE OR ALTER PROCEDURE FP_sp_Quizzes_DeleteQuestion
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @QuizId INT;
    SELECT @QuizId = QuizId FROM FP_Questions2026 WHERE Id = @Id;

    IF NOT EXISTS(SELECT 1 FROM FP_Quizzes2026 WHERE Id = @QuizId AND CreatorId = @UserId)
    BEGIN
        SELECT 0 AS Result;
        RETURN;
    END

    DELETE FROM FP_Questions2026 WHERE Id = @Id;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO

-- 5. Add Unpublish Procedure
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
