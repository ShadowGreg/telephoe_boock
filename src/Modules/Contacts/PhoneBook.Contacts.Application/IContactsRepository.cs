using PhoneBook.Contacts.Domain;

namespace PhoneBook.Contacts.Application;

public interface IContactsRepository
{
    Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct);
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Contact contact, CancellationToken ct);
    Task UpdateAsync(Contact contact, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}