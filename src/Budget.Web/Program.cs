var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapStaticAssets();

app.Run();