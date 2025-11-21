using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var isTestEnvironment = environment == "Test";

var nats = builder.AddNats("nats");

var postgresBuilder = builder.AddPostgres("budget-db");

if (!isTestEnvironment)
{
    postgresBuilder = postgresBuilder
        // Add pgAdmin
        .WithPgAdmin()
        .WithHostPort(5050)
        // Add a volume to persist data. We do NOT want this in tests, as it would make tests flaky
        .WithDataVolume(isReadOnly: false, name: "budget-development-data");
}

var postgres = postgresBuilder.AddDatabase("budgetdb");

var migrations = builder.AddProject<Budget_MigrationsRunner>("budget-migrations")
    .WithReference(postgres)
    .WaitFor(postgres);

var budgetApi = builder.AddProject<Budget_Api>("budget-api")
    .WithReference(nats)
    .WithReference(postgres)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment)
    .WaitFor(postgres)
    .WaitFor(migrations); // no need to wait for rabbitmq, as it slows down the startup

var worker = builder.AddProject<Budget_Worker>("budget-worker")
    .WithReference(nats)
    .WithReference(postgres)    
    .WaitFor(postgres)
    .WaitFor(migrations)
    .WaitFor(nats);

builder.AddProject<Budget_Ui>("budget-app")
    .WithReference(budgetApi)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment)
    .WithEnvironment("BudgetApi__BaseUrl", budgetApi.GetEndpoint("https"))
    .WaitFor(migrations)
    .WaitFor(budgetApi);

builder.Build().Run();