using System.Text.Json;
using PhoneBook.Contacts.Application;
using PhoneBook.Contacts.Domain;

namespace PhoneBook.Contacts.Infrastructure;

public sealed class JsonFileContactsRepository : IContactsRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private sealed record ContactRow(Guid Id, string Name, string Phone, string? Email, string? Notes, DateTime CreatedAt, DateTime UpdatedAt);

    private readonly ContactsFileStoreOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonFileContactsRepository(ContactsFileStoreOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            return await ReadAllUnsafeAsync(ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var all = await ReadAllUnsafeAsync(ct);
            return all.FirstOrDefault(x => x.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task AddAsync(Contact contact, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var all = (await ReadAllUnsafeAsync(ct)).ToList();
            all.Add(contact);
            await WriteAllUnsafeAsync(all, ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task UpdateAsync(Contact contact, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var all = (await ReadAllUnsafeAsync(ct)).ToList();
            var idx = all.FindIndex(x => x.Id == contact.Id);
            if (idx < 0) return;

            all[idx] = contact;
            await WriteAllUnsafeAsync(all, ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var all = (await ReadAllUnsafeAsync(ct)).ToList();
            var removed = all.RemoveAll(x => x.Id == id) > 0;
            if (!removed) return false;

            await WriteAllUnsafeAsync(all, ct);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IReadOnlyList<Contact>> ReadAllUnsafeAsync(CancellationToken ct)
    {
        EnsureDirectory();

        if (!File.Exists(_options.FilePath))
            return Array.Empty<Contact>();

        await using var stream = new FileStream(
            _options.FilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        if (stream.Length == 0)
            return Array.Empty<Contact>();

        var data = await JsonSerializer.DeserializeAsync<List<ContactRow>>(stream, JsonOptions, ct);
        if (data is null)
            return Array.Empty<Contact>();

        return data
            .Select(r => Contact.FromPersistence(r.Id, r.Name, r.Phone, r.Email, r.Notes, r.CreatedAt, r.UpdatedAt))
            .ToList();
    }

    private async Task WriteAllUnsafeAsync(List<Contact> contacts, CancellationToken ct)
    {
        EnsureDirectory();

        var dir = Path.GetDirectoryName(Path.GetFullPath(_options.FilePath))!;
        var tmp = Path.Combine(dir, $"{Path.GetFileName(_options.FilePath)}.{Guid.NewGuid():N}.tmp");

        await using (var stream = new FileStream(
            tmp,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None))
        {
            await JsonSerializer.SerializeAsync(stream, contacts, JsonOptions, ct);
            await stream.FlushAsync(ct);
        }

        if (File.Exists(_options.FilePath))
        {
            File.Replace(tmp, _options.FilePath, destinationBackupFileName: null);
        }
        else
        {
            File.Move(tmp, _options.FilePath);
        }
    }

    private void EnsureDirectory()
    {
        var full = Path.GetFullPath(_options.FilePath);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
    }
}
