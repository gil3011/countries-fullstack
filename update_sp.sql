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
