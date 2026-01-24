using Microsoft.OpenApi.Models;
using PhoneBook.Api;
using PhoneBook.Contacts.Application;
using PhoneBook.Contacts.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddContactsInfrastructure(builder.Configuration);
builder.Services.AddScoped<ContactsService>();
builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
                                 {
                                     Title = "My API",
                                     Version = "v1",
                                     Description = "PhoneBook Modular Monolith API",
                                     Contact = new OpenApiContact
                                               {
                                                   Name = "Your Name",
                                                   Email = "you@example.com"
                                               }
                                 });
    });

var app = builder.Build();

// Add startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting PhoneBook API...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Configuration sources: {Sources}", 
    string.Join(", ", builder.Configuration.Sources.Select(s => s.GetType().Name)));

try 
{
    logger.LogInformation("Running database migrations...");
    app.MigrateContactsDb();
    logger.LogInformation("Database migrations completed successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to run database migrations");
}

app.UseSerilogRequestLogging();

// Enable Swagger for both Development and Production
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PhoneBook API v1");
    options.RoutePrefix = "swagger";
});

logger.LogInformation("Mapping contacts module...");
app.MapContactsModule();


app.MapControllers();

app.Run();

public partial class Program { }