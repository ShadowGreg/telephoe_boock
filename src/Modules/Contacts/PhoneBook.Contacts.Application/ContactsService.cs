using PhoneBook.Contacts.Domain;

namespace PhoneBook.Contacts.Application;

public sealed class ContactsService
{
    private readonly IContactsRepository _repo;

    public ContactsService(IContactsRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<ContactDto>> GetAllAsync(string? q, CancellationToken ct)
    {
        var all = await _repo.GetAllAsync(ct);

        IEnumerable<Contact> filtered = all;
        if (!string.IsNullOrWhiteSpace(q))
        {
            var query = q.Trim();
            filtered = filtered.Where(x =>
                x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.Phone.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (x.Email?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return filtered
              .OrderBy(x => x.Name)
              .Select(ToDto)
              .ToList();
    }

    public async Task<ContactDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var c = await _repo.GetByIdAsync(id, ct);
        return c is null ? null : ToDto(c);
    }

    public async Task<ContactDto> CreateAsync(CreateContactRequest req, CancellationToken ct)
    {
        var contact = Contact.Create(req.Name, req.Phone, req.Email, req.Notes);
        await _repo.AddAsync(contact, ct);
        return ToDto(contact);
    }

    public async Task<ContactDto?> UpdateAsync(Guid id, UpdateContactRequest req, CancellationToken ct)
    {
        var contact = await _repo.GetByIdAsync(id, ct);
        if (contact is null) return null;

        contact.Update(req.Name, req.Phone, req.Email, req.Notes);
        await _repo.UpdateAsync(contact, ct);
        return ToDto(contact);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => _repo.DeleteAsync(id, ct);

    private static ContactDto ToDto(Contact x) =>
        new(x.Id, x.Name, x.Phone, x.Email, x.Notes, x.CreatedAt, x.UpdatedAt);
}