-- =========================================================
-- Quiz System Tables
-- =========================================================

-- 1. Quizzes Table (Represents the static quiz template created by a user)
CREATE TABLE FP_Quizzes2026 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    CreatorId INT NOT NULL,
    IsPublic BIT NOT NULL DEFAULT 0,
    Likes INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Quizzes_Creator FOREIGN KEY (CreatorId) REFERENCES FP_Users2026(Id)
);

-- 2. Questions Table
-- A question belongs to a specific quiz.
CREATE TABLE FP_Questions2026 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    QuizId INT NOT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,
    OptionA NVARCHAR(1000) NULL,
    OptionB NVARCHAR(1000) NULL,
    OptionC NVARCHAR(1000) NULL,
    OptionD NVARCHAR(1000) NULL,
    CONSTRAINT FK_Questions_Quizzes FOREIGN KEY (QuizId) REFERENCES FP_Quizzes2026(Id) ON DELETE CASCADE
);

-- 3. Quiz Attempts Table
-- Represents a specific instance of a user taking a quiz.
CREATE TABLE FP_QuizAttempts2026 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    QuizId INT NOT NULL,
    UserId INT NOT NULL,
    Score INT NOT NULL DEFAULT 0,
    DateTaken DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_QuizAttempts_Quizzes FOREIGN KEY (QuizId) REFERENCES FP_Quizzes2026(Id) ON DELETE CASCADE,
    CONSTRAINT FK_QuizAttempts_Users FOREIGN KEY (UserId) REFERENCES FP_Users2026(Id) ON DELETE CASCADE
);

-- 4. Quiz Attempt Answers Table
-- Stores the user's specific answers for each question during an attempt.
-- This maps to the Dictionary<int, string> Answers in the QuizAttempt C# class.
CREATE TABLE FP_QuizAttemptAnswers2026 (
    AttemptId INT NOT NULL,
    QuestionId INT NOT NULL,
    UserAnswer NVARCHAR(1000) NULL,
    PRIMARY KEY (AttemptId, QuestionId),
    CONSTRAINT FK_AttemptAnswers_Attempt FOREIGN KEY (AttemptId) REFERENCES FP_QuizAttempts2026(Id) ON DELETE CASCADE,
    -- We generally don't cascade delete questions if an attempt exists, but if a question is deleted, we cascade delete the answer.
    CONSTRAINT FK_AttemptAnswers_Question FOREIGN KEY (QuestionId) REFERENCES FP_Questions2026(Id)
);

-- 5. Quiz Likes Table
-- Tracks which user liked which quiz to prevent multiple likes.
CREATE TABLE FP_QuizLikes2026 (
    QuizId INT NOT NULL,
    UserId INT NOT NULL,
    PRIMARY KEY (QuizId, UserId),
    CONSTRAINT FK_QuizLikes_Quizzes FOREIGN KEY (QuizId) REFERENCES FP_Quizzes2026(Id) ON DELETE CASCADE,
    CONSTRAINT FK_QuizLikes_Users FOREIGN KEY (UserId) REFERENCES FP_Users2026(Id) ON DELETE CASCADE
);

-- 6. Quiz Countries Table
-- Junction table to associate quizzes with specific countries
CREATE TABLE FP_QuizCountries2026 (
    QuizId INT NOT NULL,
    CountryId INT NOT NULL,
    PRIMARY KEY (QuizId, CountryId),
    CONSTRAINT FK_QuizCountries_Quizzes FOREIGN KEY (QuizId) REFERENCES FP_Quizzes2026(Id) ON DELETE CASCADE,
    CONSTRAINT FK_QuizCountries_Countries FOREIGN KEY (CountryId) REFERENCES FP_Countries2026(Id) ON DELETE CASCADE
);

-- 7. Quiz Regions Table
CREATE TABLE FP_QuizRegions2026 (
    QuizId INT NOT NULL,
    Region NVARCHAR(100) NOT NULL,
    PRIMARY KEY (QuizId, Region),
    CONSTRAINT FK_QuizRegions_Quizzes FOREIGN KEY (QuizId) REFERENCES FP_Quizzes2026(Id) ON DELETE CASCADE
);
GO
