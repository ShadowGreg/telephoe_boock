using Microsoft.OpenApi.Models;
using PhoneBook.Api;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
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

        // если будут XML-комментарии
        // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        // options.IncludeXmlComments(xmlPath);
    });

var app = builder.Build();

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
            options.RoutePrefix = "swagger"; // /swagger
        });
}
app.MapContactsModule();


app.MapControllers();

app.Run();