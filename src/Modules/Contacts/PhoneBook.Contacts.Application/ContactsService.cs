using Microsoft.Extensions.Logging;
using PhoneBook.Contacts.Domain;

namespace PhoneBook.Contacts.Application;

public sealed class ContactsService
{
    private readonly IContactsRepository _repo;
    private readonly ILogger<ContactsService> _logger;

    public ContactsService(IContactsRepository repo, ILogger<ContactsService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ContactDto>> GetAllAsync(string? q, CancellationToken ct)
    {
        _logger.LogInformation("Getting all contacts with query: '{Query}'", q ?? "(empty)");
        
        try
        {
            var all = await _repo.GetAllAsync(ct);
            _logger.LogInformation("Retrieved {Count} contacts from repository", all.Count);

            IEnumerable<Contact> filtered = all;
            if (!string.IsNullOrWhiteSpace(q))
            {
                var query = q.Trim();
                filtered = filtered.Where(x =>
                    x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    x.Phone.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (x.Email?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            var result = filtered
                  .OrderBy(x => x.Name)
                  .Select(ToDto)
                  .ToList();
            
            _logger.LogInformation("Returning {Count} filtered contacts", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contacts with query: '{Query}'", q ?? "(empty)");
            throw;
        }
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