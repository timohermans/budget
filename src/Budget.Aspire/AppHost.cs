using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var nats = builder.AddNats("nats");

var postgres = builder.AddPostgres("budget-db")
    .WithPgAdmin()
    .WithHostPort(5050)
    .WithDataVolume(isReadOnly: false)
    .AddDatabase("budgetdb");

var migrations = builder.AddProject<Budget_MigrationsRunner>("budget-migrations")
    .WithReference(postgres)
    .WaitFor(postgres);

var budgetApi = builder.AddProject<Budget_Api>("budget-api")
    .WithReference(nats)
    .WithReference(postgres)
    .WaitFor(postgres)
    .WaitFor(migrations); // no need to wait for rabbitmq, as it slows down the startup

var worker = builder.AddProject<Budget_Worker>("budget-worker")
    .WithReference(nats)
    .WithReference(postgres)    
    .WaitFor(postgres)
    .WaitFor(nats);

builder.AddProject<Budget_Ui>("budget-app")
    .WithReference(budgetApi)
    .WithEnvironment("BudgetApi__BaseUrl", budgetApi.GetEndpoint("https"))
    .WaitFor(budgetApi);

builder.Build().Run();