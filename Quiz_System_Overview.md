# Quiz System Architecture Overview

This document provides a concise and focused overview of the custom Quiz feature backend architecture.

## 1. Core Entities
- **`Quiz`:** Represents the main test template. Contains properties like `Title`, `CreatorId`, `IsPublic`, `Likes`, and a list of `Questions`.
- **`Question`:** Represents a single multiple-choice question belonging to a specific `Quiz`. Includes options (A, B, C, D) and the `CorrectAnswer`.
- **`QuizAttempt`:** Represents an instance of a user taking a quiz. Stores the user's `Score`, `DateTaken`, and a dictionary mapping `QuestionId` to the user's chosen answer.

## 2. Business Rules & Security
- **Ownership Rule (IDOR Prevention):** Every modifying database operation (Update, Delete, Publish, Add Question) strictly verifies that the user performing the action is the actual creator of the quiz (`WHERE CreatorId = @UserId`).
- **Publishing Rule (Immutability):** Quizzes default to private (`IsPublic = false`). Once a user publishes a quiz, it is locked forever. The backend will actively block any further `UPDATE` or `DELETE` requests to the quiz or its questions, returning a `400 Bad Request`.
- **Client-Side Cloning:** Instead of complex backend cloning algorithms, if a user wishes to edit a public quiz, they fetch it to their frontend, modify it locally, and submit it back as a completely new quiz. This prevents database bloat from abandoned drafts.

## 3. Data Access Layer (Thick SQL Pattern)
- **Stored Procedures (`SP's Quizzes.sql`):** The heavy lifting and security validations are enforced directly at the SQL level. If a rule is violated, the SP safely affects 0 rows.
- **`DBServiceQuiz.cs`:** A unified Data Access Layer class handling all Quiz communications. It optimizes inserts (e.g., creating a quiz automatically iterates and saves all its questions in one fluid call).

## 4. API Controllers
- **`QuizController`:** Exposes RESTful endpoints to manage quizzes. It translates backend business rule violations (like trying to edit a public quiz) into clean HTTP 400 Bad Request responses.
- **`QuizAttemptController`:** Manages the exam-taking flow. Allows users to submit completed attempts and view their historical scores efficiently, omitting heavy question data when only score history is requested.
