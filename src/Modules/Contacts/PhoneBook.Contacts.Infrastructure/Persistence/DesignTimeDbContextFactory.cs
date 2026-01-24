using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PhoneBook.Contacts.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ContactsDbContext>
{
    public ContactsDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var altPath = Path.Combine(basePath, "src", "Host", "PhoneBook.Api");
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")) && Directory.Exists(altPath))
            basePath = altPath;

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conn = config.GetConnectionString("Contacts")
            ?? "Host=localhost;Port=5432;Database=phonebook;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ContactsDbContext>()
            .UseNpgsql(conn)
            .Options;

        return new ContactsDbContext(options);
    }
}
