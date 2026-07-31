using VioletManager.Application;
using VioletManager.Application.Handlers;
using VioletManager.Application.Handlers.Contacts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<CreateContactHandler>();
builder.Services.AddScoped<DeleteContactHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap the API in integration tests
public partial class Program;
