using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhoneBook.Contacts.Infrastructure.Persistence;

namespace PhoneBook.Contacts.Infrastructure;

public static class ContactsDbMigrationExtensions
{
    public static void MigrateContactsDb(this IApplicationBuilder app)
    {
        var config = app.ApplicationServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        if (!string.Equals(config.GetSection("Contacts")["Storage"], "Postgres", StringComparison.OrdinalIgnoreCase))
            return;

        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
        db.Database.Migrate();
    }
}
