# Sports Results Notifier

A .NET Worker Service that retrieves sports team statistics from a web page, displays the results in the console, and sends the collected information through email.

The project was created as part of a C#/.NET code review exercise and focuses on background services, web scraping, HTML parsing, email notifications, and clean separation of responsibilities.

## Features

* 🔄 Runs as a background Worker Service
* 🌐 Fetches sports data from an external website
* 📄 Parses HTML using **HtmlAgilityPack**
* 📊 Displays the scraped results using **Spectre.Console**
* 📧 Sends the results as an HTML email
* ⏰ Runs the task immediately and repeats every 24 hours
* 🔐 Uses environment variables for email credentials
* ⚠️ Handles errors without stopping the background worker
* 📝 Uses .NET logging for monitoring and troubleshooting

## 🛠️ Technologies Used

* **C#**
* **.NET 10**
* **Worker Service**
* **ASP.NET Core Generic Host**
* **HttpClient**
* **HtmlAgilityPack**
* **Spectre.Console**
* **SMTP / Gmail**
* **DotNetEnv**
* **Dependency Injection**
* **Async/Await**

## 🏗️ Project Structure

```text
Sangeerths.SportsResult/
│
├── baseballNotifier/
│   ├── Program.cs
│   ├── Worker.cs
│   ├── baseballNotifier.csproj
│   └── .env
│
└── baseballNotifier.slnx
```

### `Program.cs`

Responsible for configuring and starting the application.

It:

* Loads environment variables using `DotNetEnv`
* Configures the .NET Generic Host
* Registers `HttpClient`
* Registers the `Worker` as a hosted service
* Starts the application

### `Worker.cs`

Contains the main background-service workflow.

The worker is responsible for:

1. Fetching the sports webpage.
2. Parsing the HTML.
3. Extracting the required team information.
4. Displaying the results in the console.
5. Creating an HTML email report.
6. Sending the report using SMTP.
7. Waiting for the next scheduled execution.

### `.env`

Stores configuration values such as email credentials and recipient information.

Example:

```env
EMAIL_FROM_ADDRESS=your-email@gmail.com
EMAIL_APP_PASSWORD=your-app-password
EMAIL_TO_ADDRESS=recipient@example.com
```

The `.env` file should not be committed to source control.

## Reflection

This project helped reinforce several practical .NET concepts, especially **Worker Services and background processing**.

One of the main learning points was understanding how a `BackgroundService` works inside the .NET Generic Host. Instead of running the scraping logic once and exiting, the application can stay alive and execute the task periodically.

Using `PeriodicTimer` also provided a simple way to schedule repeated execution without manually managing threads.

Another important part was working with **HTML parsing**. `HttpClient` is used to retrieve the page, while `HtmlAgilityPack` makes it possible to navigate the returned HTML and extract the required information.

The project also provided experience with:

* Dependency Injection
* Environment-based configuration
* SMTP email
* HTML email generation
* Exception handling
* Logging
* Console formatting with Spectre.Console
* Asynchronous programming

A particularly useful lesson was keeping sensitive information such as email credentials outside the source code. Using environment variables makes the application safer and allows configuration to change without modifying the application itself.

## Architectural Choices

### Worker Service

The project uses the .NET **Worker Service** model because the application is designed to run continuously in the background rather than respond to HTTP requests.

```text
Generic Host
     │
     ▼
Background Worker
     │
     ├── Fetch data
     │
     ├── Parse data
     │
     ├── Display results
     │
     └── Send email
```

This makes `BackgroundService` a natural fit for the application's requirements.

### Dependency Injection

`HttpClient` and the worker are registered through the built-in .NET Dependency Injection system.

This avoids manually creating dependencies inside the worker and makes the application easier to maintain and test.

### `HttpClient`

`HttpClient` is used to retrieve the external webpage.

It is registered through dependency injection rather than creating a new `HttpClient` for every request, following the recommended .NET approach for managing HTTP clients.

### HtmlAgilityPack

The application uses **HtmlAgilityPack** instead of attempting to parse HTML using string operations or regular expressions.

This provides a more appropriate way to navigate the HTML document and locate the required elements.

### PeriodicTimer

The application uses `PeriodicTimer` to control the execution interval.

The workflow is:

```text
Application starts
       ↓
Run scraping task
       ↓
Send email
       ↓
Wait 24 hours
       ↓
Run scraping task again
       ↓
Repeat
```

This keeps the scheduling logic simple and easy to understand.

### Environment Configuration

Email credentials are loaded from environment variables rather than being hardcoded.

```text
.env
 │
 ├── EMAIL_FROM_ADDRESS
 ├── EMAIL_APP_PASSWORD
 └── EMAIL_TO_ADDRESS
```

This separates application configuration from application code and prevents credentials from being directly embedded in the source.

### Error Handling

The scraping and email workflow is wrapped in exception handling so that a temporary failure does not immediately terminate the background worker.

Errors are recorded through the built-in logging system, making it easier to diagnose problems while the worker continues running.

### Console Presentation

**Spectre.Console** is used to present the scraped information in a readable table and provide visual feedback about the application's current state.

This makes the console application easier to monitor while it is running.

## How to Run

### 1. Clone the repository

```bash
git clone https://github.com/the-csharp-academy/CodeReviews.Console.SportsResults.git
```

### 2. Navigate to the project

```bash
cd CodeReviews.Console.SportsResults
```

### 3. Configure environment variables

Create a `.env` file containing:

```env
EMAIL_FROM_ADDRESS=your-email@gmail.com
EMAIL_APP_PASSWORD=your-app-password
EMAIL_TO_ADDRESS=recipient@example.com
```

### 4. Restore dependencies

```bash
dotnet restore
```

### 5. Run the application

```bash
dotnet run
```

The application will immediately retrieve the sports data, display it in the console, send the email report, and then wait for the next scheduled execution.

## Future Improvements

Some possible improvements include:

* Move scraping logic into a dedicated service
* Move email functionality into a dedicated service
* Add unit tests for the HTML parsing logic
* Make the scraping URL configurable
* Make the execution interval configurable
* Add retry logic for network failures
* Support multiple sports data sources
* Add structured configuration using `IOptions`
* Add more detailed logging
* Store historical results
* Add support for additional email providers
