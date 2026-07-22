# 🧠 User Actions on Quizzes Overview

This document provides a quick, easy-to-read overview of all the actions a user can perform regarding **Quizzes** in the system.

## 🧭 Quiz Exploration

| Action | Description | Endpoint |
| :--- | :--- | :--- |
| **View Public Quizzes** | Retrieve the list of all published quizzes available for taking. | `GET /api/Quiz/Public` |
| **View Specific Quiz** | Get the full details and questions of a specific quiz by its ID. | `GET /api/Quiz/{id}` |
| **View User's Quizzes** | View all quizzes (public and private) created by a specific user. | `GET /api/Quiz/User/{userId}` |

---

## 🛠️ Quiz Creation & Management *(Creator Only)*
*Note: Modifying a quiz is only allowed if `IsPublic = false`.*

| Action | Description | Endpoint |
| :--- | :--- | :--- |
| **Create Quiz** | Create a brand new quiz template. | `POST /api/Quiz` |
| **Update Quiz Title** | Rename a private quiz. | `PUT /api/Quiz/{id}?userId={userId}` |
| **Delete Quiz** | Completely delete a private quiz. | `DELETE /api/Quiz/{id}?userId={userId}` |
| **Publish Quiz** | Lock the quiz and make it public for everyone. *(Cannot be undone)* | `POST /api/Quiz/{id}/Publish?userId={userId}` |

---

## 📝 Question Management *(Creator Only)*
*Note: Modifying questions is only allowed if the parent quiz is private (`IsPublic = false`).*

| Action | Description | Endpoint |
| :--- | :--- | :--- |
| **Add Question** | Add a new question to an existing private quiz. | `POST /api/Quiz/{id}/Question?userId={userId}` |
| **Update Question** | Modify the text or options of an existing question. | `PUT /api/Quiz/Question/{questionId}?userId={userId}` |
| **Delete Question** | Remove a question from the quiz. | `DELETE /api/Quiz/Question/{questionId}?userId={userId}` |

---

## 🎮 Taking Quizzes (Attempts)

| Action | Description | Endpoint |
| :--- | :--- | :--- |
| **Submit Attempt** | Submit answers and score after taking a quiz. | `POST /api/QuizAttempt` |
| **View Attempt Details**| View exactly what answers a user chose in a past attempt. | `GET /api/QuizAttempt/{id}` |
| **View Attempt History**| Get the full history of a user's past quiz scores. | `GET /api/QuizAttempt/User/{userId}` |
| **Delete Attempt** | Delete a past quiz attempt from history. | `DELETE /api/QuizAttempt/{id}?userId={userId}` |

---

## ❤️ Social Interactions

| Action | Description | Endpoint |
| :--- | :--- | :--- |
| **Like a Quiz** | Add a 'Like' to a public quiz to boost its popularity. | `POST /api/Quiz/{id}/Like` |
