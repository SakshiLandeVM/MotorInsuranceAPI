# MotorInsuranceAPI

A motor insurance claim management REST API built using ASP.NET Core Web API and C# with .NET 8. The application supports user authentication, claim submission, claim management, and administrative claim approval or rejection.

## Features

- **User authentication:** User registration and login.
- **JWT authentication:** Secure access to protected API endpoints.
- **Role-based authorization:** Restrict administrative operations to Admin users.
- **Motor insurance claims:** Submit and retrieve claims.
- **Image upload:** Receive and store claim-related images.
- **Admin claim management:** View claims and approve or reject them.
- **Email notifications:** Send email notifications when an administrator updates a claim's status.
- **Pagination and sorting:** Retrieve claims in manageable pages and sort results.
- **Entity Framework Core:** Interact with the SQL Server database.
- **Swagger UI:** Explore and test API endpoints.

## Technologies Used

- C#
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- Microsoft SQL Server / SQL Server LocalDB
- JWT Bearer Authentication
- AutoMapper
- Swagger / OpenAPI
- DotNetEnv
- SMTP email notifications

## Project Structure

```text
MotorInsuranceAPI/
├── Controllers/       # API endpoints
├── Models/            # Entity models
├── DTOs/               # Request and response objects
├── Data/               # Database context and related code
├── Services/           # Business logic and supporting services
├── Mapping/            # AutoMapper profiles for mapping entities and DTOs
├── Migrations/         # Entity Framework Core migrations
├── wwwroot/            # Static files, if configured
├── Program.cs          # Application configuration and startup
├── appsettings.json    # Application configuration
├── .gitignore          # Files excluded from Git
└── README.md            # Project documentation
```

*Folder names may vary depending on the current project structure.*

## Prerequisites

Install the following before running the application:

- .NET 8 SDK
- SQL Server LocalDB or another supported SQL Server instance
- Visual Studio or Visual Studio Code
- Git

## Configuration

1. Clone this repository.
2. Configure the database connection string in your local configuration.
3. Configure the required JWT settings.
4. Configure administrator seeding settings, if required.
5. Configure SMTP email settings if email notifications are needed.
6. Keep credentials and secret values in environment variables or another secure local configuration source. Never commit your actual `.env` file or production secrets.

Use the configuration keys expected by the application. Create your own local `.env` file based on the variables required by the code.

## Run the Application

Open a terminal in the project directory and run:

```bash
dotnet restore
dotnet build
dotnet run
```

Use the local URL printed in the terminal to access the running application.

In Development mode, open the Swagger UI at:

```text
/swagger
```

For example, if the application starts at `http://localhost:5184`, Swagger UI is available at `http://localhost:5184/swagger`.

The port may be different on your machine.

## Database

The application uses Entity Framework Core to communicate with SQL Server.

If migrations are included, apply them using the appropriate connection string and migration setup for your environment. For example:

```bash
dotnet ef database update
```

This command requires the Entity Framework Core CLI tool and the appropriate design-time configuration.

