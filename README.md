# PartnerIntegrationBFFService

Summary
- BFF service that validates partner transactions, verifies partner via external Partner API, and publishes transaction messages to Kafka.
- Projects:
  - `PartnerIntegrationBFFService.API` — ASP.NET Core Web API (entry point)
  - `PartnerIntegrationBFFService.Application` — domain logic, MediatR handlers, validators, pipeline behaviors
  - `PartnerIntegrationBFFService.Infrastructure` — HTTP clients, Kafka producer, DI wiring
  - `PartnerIntegrationBFFService.UnitTests` — test projects

Project structure (important files)
- `PartnerIntegrationBFFService.API/Program.cs` — application startup (registers services, middleware, controllers)
- `PartnerIntegrationBFFService.API/DependencyInjection.cs` — API-level DI helpers (Serilog)
- `PartnerIntegrationBFFService.API/Controllers/Mock/PartnerVerificationMockController.cs` — local mock partner verification endpoint
- `PartnerIntegrationBFFService.Application/DependencyInjection.cs` — registers MediatR, validators and pipeline behaviors
- `PartnerIntegrationBFFService.Application/Features/Partners/Commands/PartnerTransactions/*` — command, validator, handler
- `PartnerIntegrationBFFService.Infrastructure/DependencyInjection.cs` — registers `IPartnerVerificationService` and Kafka producer
- `PartnerIntegrationBFFService.Infrastructure/Messaging/TransactionMessageProducer.cs` — Kafka publish + retry logic
- `PartnerIntegrationBFFService.API/appsettings.json` — runtime configuration
- Tests: `PartnerIntegrationBFFService.UnitTests/*` — unit tests

Request flow (single transaction request)
1. Client POSTs `PartnerTransactionsCommand` to `POST /api/v1/Partner/transactions` (controller in `API.Controllers.V1.PartnerController`).
2. Controller sends command to MediatR (`ISender.Send`).
3. MediatR pipeline:
   - `ValidationBehavior<TRequest,TResponse>` runs validators (FluentValidation). If validation fails a `ValidationException` is thrown.
   - `LoggingBehavior<TRequest,TResponse>` logs start/end and performance.
4. Handler: `PartnerTransactionsHandler.Handle(...)`
   - Calls `IPartnerVerificationService.VerifyPartnerAsync(partnerId)` (typed `HttpClient` in `Infrastructure`).
   - If verification returns false → throws `BadRequestException`.
   - If verified → calls `IMessageProducer<PartnerTransactionsCommand>.PublishAsync(...)` (Kafka producer).
   - On producer failures the handler maps exceptions to `BadRequestException` (so they surface as 400 via middleware).
5. `ExceptionMiddleware` maps exceptions to HTTP ProblemDetails and appropriate status codes.

Configuration (`appsettings.json`) — keys and meaning
- `Logging.LogLevel` — ASP.NET log levels.
- `PartnerApi.BaseUrl` — base address used by `PartnerVerificationService` typed `HttpClient`. When running locally point to the mock server (or real partner API).
- `Kafka`
  - `BootstrapServers` — Kafka bootstrap server(s), e.g. `localhost:9092` or the docker service name `kafka:9092`.
  - `Topic` — topic to publish partner transactions to.
- `Serilog` — Serilog configuration (sinks, minimum levels, enrichment). Adjust to add Loki or other sinks.
- `Kestrel.Endpoints` — local binding URLs (HTTP, gRPC).
- `AllowedHosts` — allowed hostnames.

Docker Compose (local dev)
- `docker-compose.yml` included: it defines `kafka` and `partner-bff-api`. The API service sets `Kafka__BootstrapServers=kafka:9092`.
- Prerequisites: Docker Desktop / Docker Engine installed and running.
- Start services:
  - __docker-compose up -d__
- Verify:
  - API should be reachable at `http://localhost:5206` (unless ports changed).
  - Kafka will be at `localhost:9092` for host mapping, or `kafka:9092` inside the compose network.

Unit tests, coverage and report generation
- Required test packages (ensure present in the test project `.csproj`):
  - `Microsoft.NET.Test.Sdk`
  - `xunit`
  - `xunit.runner.visualstudio`
  - `coverlet.collector` (for coverage collection)
  - `Moq` (for mocking)
  - `Microsoft.AspNetCore.Mvc.Testing` (if you need to boot the API in tests)
- Run tests:
  - Unit tests: __dotnet test PartnerIntegrationBFFService.UnitTests/ --no-build__
  - All tests with coverage (XPlat collector): __dotnet test --collect:"XPlat Code Coverage"__
  - Or classic coverlet CLI style:
    - __dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./TestResults/coverage.cobertura.xml__
- Produce an HTML report (install `ReportGenerator` global tool if not installed):
  - __dotnet tool install -g dotnet-reportgenerator-globaltool__
  - Generate report:
    - __reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html__
  - Open `coverage-report/index.htm` in a browser.

Viewing test results inside Visual Studio
- Use __Test Explorer__ to run tests.
- To collect code coverage in Visual Studio: use __Test > Analyze Code Coverage__ or configure Coverage via the test run settings. Also ensure the API project sets `<PreserveCompilationContext>true</PreserveCompilationContext>` if you use `WebApplicationFactory<Program>` in tests.

Common troubleshooting notes
- Web application factory: if the app uses top-level statements, add a file with a public partial Program:
  - `PartnerIntegrationBFFService.API/ProgramEntry.cs`
    ```csharp
    // minimal entry type so WebApplicationFactory<Program> can locate the entry point
    public partial class Program { }
    ```
- If `WebApplicationFactory` complains about missing `.deps.json`, make sure the API project contains:
  - `<PreserveCompilationContext>true</PreserveCompilationContext>` and `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` in the API `.csproj`, and build before running tests.
- If mocking `ILogger<T>` with Moq fails due to access restrictions, keep any fake request/response types `public` (or use `InternalsVisibleTo` for `DynamicProxyGenAssembly2`).

Quick checklist to produce coverage reports locally
1. Ensure projects build: __dotnet build__
2. Run tests and collect coverage: __dotnet test --collect:"XPlat Code Coverage"__
3. Locate coverage file (inside `PartnerIntegrationBFFService.UnitTests/TestResults/<run-id>/coverage.cobertura.xml`)
4. Generate HTML: __reportgenerator -reports:"PartnerIntegrationBFFService.UnitTests/TestResults/*/coverage.cobertura.xml" -targetdir:"coverage-report"__
5. Open `coverage-report/index.htm`