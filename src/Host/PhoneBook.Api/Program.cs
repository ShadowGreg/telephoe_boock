using Microsoft.OpenApi.Models;
using PhoneBook.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
            options.RoutePrefix = "swagger";
        });
}
app.MapContactsModule();


app.MapControllers();

app.Run();