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
