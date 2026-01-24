using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhoneBook.Contacts.Application;
using PhoneBook.Contacts.Domain;
using PhoneBook.Contacts.Infrastructure.Persistence;

namespace PhoneBook.Contacts.Infrastructure;

public sealed class PostgresContactsRepository : IContactsRepository
{
    private readonly ContactsDbContext _db;
    private readonly ILogger<PostgresContactsRepository> _logger;

    public PostgresContactsRepository(ContactsDbContext db, ILogger<PostgresContactsRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct)
    {
        _logger.LogInformation("Getting all contacts from PostgreSQL database");
        
        try
        {
            _logger.LogDebug("Executing query to get all contacts ordered by name");
            var list = await _db.Contacts.OrderBy(x => x.Name).ToListAsync(ct);
            _logger.LogInformation("Successfully retrieved {Count} contacts from database", list.Count);
            return list.Select(ToDomain).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts from PostgreSQL database");
            throw;
        }
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var e = await _db.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : ToDomain(e);
    }

    public async Task AddAsync(Contact contact, CancellationToken ct)
    {
        _db.Contacts.Add(ToEntity(contact));
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Contact contact, CancellationToken ct)
    {
        var e = await _db.Contacts.FirstOrDefaultAsync(x => x.Id == contact.Id, ct);
        if (e is null) return;

        e.Name = contact.Name;
        e.Phone = contact.Phone;
        e.Email = contact.Email;
        e.Notes = contact.Notes;
        e.UpdatedAt = contact.UpdatedAt;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var e = await _db.Contacts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;

        _db.Contacts.Remove(e);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static Contact ToDomain(ContactEntity e) =>
        Contact.FromPersistence(e.Id, e.Name, e.Phone, e.Email, e.Notes, e.CreatedAt, e.UpdatedAt);

    private static ContactEntity ToEntity(Contact c) =>
        new()
        {
            Id = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            Email = c.Email,
            Notes = c.Notes,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
}
