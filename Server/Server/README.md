# countries-fullstack

A full-stack web application for exploring countries, managing personal travel lists, sharing country-related content, playing quizzes, and administering users and system activity.

The project was developed as part of the **Server-Side Web Information Systems** course.

---

## Project Overview

`countries-fullstack` is an information system centered around countries of the world.

Country data is retrieved on the server from the **REST Countries API**, processed by the ASP.NET Core backend, and stored in a Microsoft SQL Server database.

The system includes:

- User registration, login, logout, and profile management
- Personal language and continent preferences
- Country browsing, filtering, sorting, and detailed country pages
- Personal “Visited” and “Wishlist” country lists
- User-created recommendations, thoughts, and reviews
- Timed country quizzes and score tracking
- Administrator tools for managing users and viewing system statistics
- Google Gemini integration
- Server-side logging and middleware

---

## Technologies

### Backend

- C#
- .NET 6
- ASP.NET Core Web API
- Microsoft SQL Server
- ADO.NET
- Stored Procedures
- REST APIs
- Google Gemini API

### Frontend

- HTML5
- CSS3
- JavaScript
- jQuery
- AJAX
- Local Storage

### Development Tools

- Visual Studio 2022
- SQL Server Management Studio
- Swagger / OpenAPI
- Git
- GitHub

---

## Architecture

The project follows a three-layer architecture.

### Presentation Layer

The client is implemented with HTML, CSS, JavaScript, jQuery, and AJAX.

Its responsibilities include:

- Rendering country lists and country details
- Handling registration and login forms
- Managing user preferences
- Displaying personal travel lists
- Creating, updating, and deleting shares
- Running the quiz interface
- Displaying the administrator dashboard
- Sending HTTP requests to the server API

### Business Logic Layer

The `BL` layer contains the application’s models and business rules.

Its responsibilities include:

- Validating application data
- Coordinating between controllers and the data-access layer
- Applying business logic for users, countries, shares, quizzes, and administrator actions

### Data Access Layer

The `DAL` layer communicates directly with Microsoft SQL Server.

It uses:

- `SqlConnection`
- `SqlCommand`
- `SqlDataReader`
- Stored Procedures
- Dedicated `DBService` classes

Database access is organized into classes such as:

- `DBServiceBase`
- `DBServiceUser`
- `DBServiceCountry`
- `DBServiceShare`
- `DBServiceQuiz`

`DBServiceBase` contains shared connection and command-creation functionality.

---

## Backend Structure

```text
Server
├── BL
├── Controller
│   ├── AdminController.cs
│   ├── CountryController.cs
│   ├── GeminiController.cs
│   ├── QuizAttemptController.cs
│   ├── QuizController.cs
│   ├── ShareController.cs
│   └── UserController.cs
├── DAL
│   ├── DBServiceBase.cs
│   ├── DBServiceCountry.cs
│   ├── DBServiceQuiz.cs
│   ├── DBServiceShare.cs
│   └── DBServiceUser.cs
├── DTO
├── Logging
├── Logs
├── Middleware
├── Services
├── .env.example
├── appsettings.json
└── Program.cs
```

---

## Frontend Structure

```text
Client
├── CSS
│   ├── admin.css
│   ├── country.css
│   ├── createShare.css
│   ├── indexStyle.css
│   ├── loginStyle.css
│   ├── NavBar.css
│   ├── shares.css
│   ├── user_quiz.css
│   └── userHomeStyle.css
├── JS
│   ├── admin.js
│   ├── ajaxCalls.js
│   ├── apiRoutes.js
│   ├── country.js
│   ├── createShare.js
│   ├── indexScript.js
│   ├── login.js
│   ├── navbar.js
│   ├── shares.js
│   ├── temp.js
│   ├── user_quiz.js
│   └── userHome.js
└── Pages
    ├── admin.html
    ├── country.html
    ├── fetchCountries.html
    ├── index.html
    ├── login.html
    ├── navbar.html
    ├── shares.html
    ├── user_quiz.html
    └── userHome.html
```

---

## Main Features

### User Management

Users can:

- Register a new account
- Log in and log out
- Update personal details
- Save personal continent preferences
- Save spoken languages and proficiency levels

### Countries

The application supports:

- Importing country data from the REST Countries API
- Saving country data in SQL Server
- Viewing all countries
- Viewing a detailed page for a selected country
- Searching, filtering, and sorting countries
- Filtering by fields such as name, region, language, currency, population, and area

### Personal Country Lists

Authenticated users can:

- Add countries to a wishlist
- Mark countries as visited
- Remove countries from personal lists
- Move countries between lists
- View their saved countries

### Shares

Users can publish country-related content, including:

- Recommendations
- Thoughts
- Reviews

Users can also:

- View shares created by other users
- View all shares connected to a specific country
- Edit their own shares
- Delete their own shares

### Quizzes

The project includes timed country quizzes.

The quiz system supports:

- Quiz questions
- Time limits
- Score calculation
- User attempts
- Saving quiz results

### Administrator Dashboard

Administrators use the regular login page and receive access according to their role.

The administrator dashboard supports:

- Viewing all users
- Blocking and unblocking users
- Promoting and demoting administrators
- Enabling or disabling sharing permissions
- Viewing daily login activity
- Viewing imported-country statistics
- Viewing wishlist and visited-country statistics
- Viewing the number of shares created

---

## REST Countries API

The server retrieves country information from the REST Countries API.

The retrieved data is mapped to the project’s country model and stored in Microsoft SQL Server.

The `fetchCountries.html` page is used for the initial country-data import.

This import page should be used carefully to avoid inserting duplicate data.

---

## Google Gemini Integration

The project integrates with Google Gemini to dynamically generate country-related quizzes according to settings selected by the user.

The user sends quiz preferences, such as the requested topic and other quiz configuration options. These settings are sent to the server, which forwards them to the Gemini service.

Gemini then generates a quiz that matches the selected criteria. The generated questions are returned through the API and displayed to the user in the quiz interface.

The integration is implemented through:

- `GeminiController`
- Service classes under the `Services` folder
- Environment-based configuration
- Client requests containing the user's quiz settings

The general flow is:

1. The user selects the desired quiz settings.
2. The client sends the selected settings to the server.
3. `GeminiController` receives the request.
4. The Gemini service creates quiz questions related to the selected topic.
5. The generated quiz is returned to the client.
6. The user plays the generated quiz and receives a score.

Sensitive values, such as the Gemini API key, must not be committed to GitHub.

The repository includes an `.env.example` file that demonstrates the required environment-variable structure without exposing private credentials.

Example:

```env
GEMINI_API_KEY=your_api_key_here
```

---

## Logging

The server contains a centralized logging mechanism organized under:

- `Logging`
- `Middleware`
- `Logs`

The logging system is used to record server activity and errors for debugging and monitoring.

Sensitive data such as passwords, API keys, and private user information should not be written to log files.

---

## Database

The project uses Microsoft SQL Server.

Database access is implemented with ADO.NET and Stored Procedures rather than Entity Framework Core.

Each database operation is handled through:

1. A Stored Procedure in SQL Server
2. A matching method in a `DBService` class
3. A business-logic method
4. A controller endpoint

### Main Database Areas

The database contains data related to:

- Users
- Countries
- Continents
- Languages
- User language preferences
- User continent preferences
- Wishlist countries
- Visited countries
- Shares
- Quiz questions
- Quiz attempts
- Login activity

---

## Configuration

### Connection String

Configure the SQL Server connection string in `appsettings.json` or `appsettings.Development.json`.

Example:

```json
{
  "ConnectionStrings": {
    "myProjDB": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

Use the connection-string name expected by `DBServiceBase`.

Do not commit real database credentials to a public repository.

### Environment Variables

Create a `.env` file based on `.env.example`.

Example:

```env
GEMINI_API_KEY=your_api_key_here
```

The real `.env` file should remain excluded through `.gitignore`.

---

## Installation and Running

### 1. Clone the Repository

```bash
git clone https://github.com/gil3011/countries-fullstack.git
cd countries-fullstack
```

### 2. Configure the Database

- Create or select the required SQL Server database
- Create the required tables
- Run the project’s Stored Procedure scripts
- Update the connection string in `appsettings.json` or `appsettings.Development.json`

### 3. Configure Gemini

- Copy `.env.example`
- Rename the copy to `.env`
- Add a valid Gemini API key

### 4. Run the Server

Using Visual Studio:

1. Open the solution
2. Select the `Server` project
3. Run the project

Using the .NET CLI:

```bash
cd Server
dotnet restore
dotnet run
```

### 5. Open Swagger

After starting the server, open the Swagger URL shown in Visual Studio or in the terminal output.

Example:

```text
https://localhost:<port>/swagger
```

### 6. Run the Client

Open the client through a local web server, Visual Studio, or the deployed course server.

Do not open all pages directly through `file://`, because AJAX requests may be blocked by browser security rules.

### 7. Import Countries

Open:

```text
Pages/fetchCountries.html
```

Use the import action to retrieve country data and save it in the database.

---

## API Organization

The API is divided into controllers by domain:

- `UserController`
- `CountryController`
- `ShareController`
- `QuizController`
- `QuizAttemptController`
- `AdminController`
- `GeminiController`

The exact routes and request formats can be viewed through Swagger.

---

## Security Notes

- Do not commit `.env`
- Do not commit real API keys
- Do not commit production database passwords
- Keep `appsettings.json` free of private production credentials
- Validate user input on the server
- Avoid logging passwords or secret values
- Restrict administrator operations according to user role

---

## Git Workflow

The project uses a Git-based workflow.

Recommended process:

1. Update the local `develop` branch
2. Create a feature branch
3. Implement and test the feature
4. Commit the changes
5. Push the feature branch
6. Open a Pull Request into `develop`
7. Resolve conflicts if necessary
8. Merge after review

Example:

```bash
git checkout develop
git pull origin develop
git checkout -b feature/example-feature
git add .
git commit -m "Add example feature"
git push -u origin feature/example-feature
```

---

## Notes

- The project does not use Entity Framework Core.
- All SQL operations are implemented through Stored Procedures and `DBService` classes.
- The client and server are separate parts of the system.
- Swagger should be used to verify and test server endpoints.
- The actual server port may differ between development environments.
- Database scripts and environment configuration must be prepared before running the full system.

---

## Team

- Gil Alon
- Eyal Davidov
- Ron Omer

---

## Repository

```text
https://github.com/gil3011/countries-fullstack
```
