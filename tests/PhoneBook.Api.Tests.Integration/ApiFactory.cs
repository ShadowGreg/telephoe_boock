using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoneBook.Contacts.Application;
using PhoneBook.Contacts.Infrastructure;

namespace PhoneBook.Api.Tests.Integration;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public string TempFilePath { get; } = Path.Combine(
        Path.GetTempPath(),
        "phonebook-it-" + Guid.NewGuid().ToString("N") + ".json");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Contacts:Storage"] = "JsonFile",
                ["ContactsFileStore:FilePath"] = TempFilePath,
                ["Serilog:MinimumLevel:Default"] = "Fatal"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IContactsRepository>();
            services.AddSingleton<IContactsRepository>(new JsonFileContactsRepository(
                new ContactsFileStoreOptions { FilePath = TempFilePath }));
        });
    }
}
