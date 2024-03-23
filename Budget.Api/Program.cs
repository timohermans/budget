using Budget.Api;
using Microsoft.AspNetCore.Mvc;

[assembly: ApiController]
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services
    .AddDatabase(config)
    .AddEntraAuthentication(config.GetSection("AzureAd"))
    .AddUseCases()
    .AddSwagger(config.GetSection("Swagger"))
    .AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseBudgetApi(app.Environment.IsDevelopment(), app.Configuration);

app.Run();