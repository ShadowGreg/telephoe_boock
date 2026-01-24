using Microsoft.EntityFrameworkCore;
using PhoneBook.Contacts.Application;
using PhoneBook.Contacts.Domain;
using PhoneBook.Contacts.Infrastructure.Persistence;

namespace PhoneBook.Contacts.Infrastructure;

public sealed class PostgresContactsRepository : IContactsRepository
{
    private readonly ContactsDbContext _db;

    public PostgresContactsRepository(ContactsDbContext db) => _db = db;

    public async Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct)
    {
        var list = await _db.Contacts.OrderBy(x => x.Name).ToListAsync(ct);
        return list.Select(ToDomain).ToList();
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
