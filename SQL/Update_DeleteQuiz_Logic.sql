-- Run this script in your SQL environment to update the Delete Quiz logic.
-- This removes the restriction that prevented public quizzes from being deleted.
-- Because of the ON DELETE CASCADE constraint on FP_QuizAttempts2026, 
-- deleting a quiz will automatically delete all associated attempts.

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
