using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoneBook.Contacts.Application;

namespace PhoneBook.Contacts.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContactsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = new ContactsFileStoreOptions();
        configuration.GetSection("ContactsFileStore").Bind(options);

        services.AddSingleton(options);
        services.AddSingleton<IContactsRepository, JsonFileContactsRepository>();

        return services;
    }
}