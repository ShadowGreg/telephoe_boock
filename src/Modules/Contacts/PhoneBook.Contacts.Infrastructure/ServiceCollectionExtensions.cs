using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoneBook.Contacts.Application;
using PhoneBook.Contacts.Infrastructure.Persistence;

namespace PhoneBook.Contacts.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContactsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var storage = configuration.GetValue<string>("Contacts:Storage") ?? "JsonFile";

        if (string.Equals(storage, "Postgres", StringComparison.OrdinalIgnoreCase))
        {
            var conn = configuration.GetConnectionString("Contacts")
                ?? throw new InvalidOperationException("Contacts:Storage=Postgres requires ConnectionStrings:Contacts.");

            services.AddDbContext<ContactsDbContext>(o => o.UseNpgsql(conn));
            services.AddScoped<IContactsRepository, PostgresContactsRepository>();
        }
        else
        {
            var options = new ContactsFileStoreOptions();
            configuration.GetSection("ContactsFileStore").Bind(options);
            services.AddSingleton(options);
            services.AddSingleton<IContactsRepository, JsonFileContactsRepository>();
        }

        return services;
    }
}