# countries-fullstack

A full-stack web application for exploring countries, managing travel lists, sharing country-related content, playing quizzes, and administering users and system activity.

Developed as part of the **Server-Side Web Information Systems** course.

## Overview

Country data is retrieved from the **REST Countries API**, processed by an ASP.NET Core backend, and stored in Microsoft SQL Server.

Main capabilities:
- User registration, login, logout, and profile management
- Country browsing, filtering, sorting, detailed country pages, and an interactive map
- Personal visited and wishlist lists
- Recommendations, thoughts, and reviews
- Timed quizzes with score tracking
- Administrator tools and statistics
- Google Gemini quiz generation and personalized country recommendations
- Server-side logging and middleware

## Technologies

### Backend
- C#, .NET 6, ASP.NET Core Web API
- Microsoft SQL Server
- ADO.NET and Stored Procedures
- REST APIs
- Google Gemini API

### Frontend
- HTML5, CSS3, JavaScript
- jQuery and AJAX
- Local Storage

### Tools
- Visual Studio 2022
- SQL Server Management Studio
- Swagger / OpenAPI
- Git and GitHub

## Architecture

The project follows a three-layer architecture:
- **Controllers Layer** – Handles HTTP requests and exposes CRUD operations through API endpoints
- **Business Logic Layer** – Models, validation, and application logic
- **Data Access Layer** – ADO.NET, Stored Procedures, and DBService classes

Database flow:
1. Stored Procedure
2. DBService method
3. Business Logic method
4. Controller endpoint

## Project Structure

```text
countries-fullstack
├── Server
│   ├── BL
│   ├── Controller
│   ├── DAL
│   ├── DTO
│   ├── Logging
│   ├── Middleware
│   ├── Services
│   └── Program.cs
└── Client
    ├── CSS
    ├── JS
    └── Pages
```

Main controllers:
- `UserController`
- `CountryController`
- `ShareController`
- `QuizController`
- `QuizAttemptController`
- `AdminController`
- `GeminiController`

Routes and request formats can be explored through Swagger.

## Main Features

### Users
Users can register, log in, update personal details, and save continent and language preferences.

### Countries
- Import and store country data
- View all countries and detailed country pages
- Explore countries through an interactive map
- Display visited and wishlist countries on the map
- Search, filter, and sort countries
- Filter by name, region, language, currency, population, and area

### Personal Lists
Authenticated users can add, remove, and move countries between visited and wishlist lists.

### Shares
Users can publish recommendations, thoughts, and reviews. They can also view public and country-specific shares, and edit or delete their own content.

### Quizzes
The system supports timed multiple-choice quizzes, score calculation, saved attempts, and result tracking.

### Administration
Administrators can:
- View all users
- Block and unblock users
- Promote and demote administrators
- Enable or disable sharing permissions
- View login, country, wishlist, visited, and share statistics

## Google Gemini

Google Gemini is used for two main features:

- Generating country-related quizzes from settings selected by the user
- Creating personalized recommendations for countries the user may want to visit

Quiz flow:
1. The user selects quiz settings
2. The client sends them to `GeminiController`
3. Gemini generates the questions
4. The quiz is returned to the client
5. The result is calculated and saved

Recommendation flow:
1. The client sends the user's preferences to the server
2. Gemini analyzes the selected preferences
3. Recommended countries and explanations are returned to the client

The API key is stored in a local `.env` file.

## Configuration

### Database

Configure the SQL Server connection string in `appsettings.json` or `appsettings.Development.json`.

```json
{
  "ConnectionStrings": {
    "myProjDB": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### Environment Variables

Create `.env` from `.env.example`:

```env
GEMINI_API_KEY=your_api_key_here
```

Do not commit real credentials.

## Installation

### 1. Clone

```bash
git clone https://github.com/gil3011/countries-fullstack.git
cd countries-fullstack
```

### 2. Configure
- Create or select the SQL Server database
- Create the required tables
- Run the Stored Procedure scripts
- Update the connection string
- Create `.env` and add the Gemini API key

### 3. Run the server

```bash
cd Server
dotnet restore
dotnet run
```

Swagger is available at the URL shown after startup.

### 4. Run the client

Open the client through a local web server, Visual Studio, or the deployed course server.

Avoid using `file://`, because browser security rules may block AJAX requests.

### 5. Import countries

Open:

```text
Pages/fetchCountries.html
```

Use the import action carefully to avoid duplicate data.

## Logging and Security

The server includes centralized logging and middleware for recording activity and errors.

- Do not commit `.env`, API keys, or production passwords
- Validate user input on the server
- Avoid logging passwords and secret values
- Restrict administrator operations by role

## Team
- Gil Alon
- Eyal Davidov
- Ron Omer

## Repository

```text
https://github.com/gil3011/countries-fullstack
```
