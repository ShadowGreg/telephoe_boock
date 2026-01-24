namespace PhoneBook.Contacts.Domain;
public sealed class Contact {
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Contact() { }

    public static Contact Create(string name, string phone, string? email, string? notes) {
        Validate(name, phone, email);

        var now = DateTime.UtcNow;
        return new Contact {
                               Id = Guid.NewGuid(),
                               Name = name.Trim(),
                               Phone = phone.Trim(),
                               Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                               Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                               CreatedAt = now,
                               UpdatedAt = now
                           };
    }

    public static Contact FromPersistence(Guid id, string name, string phone, string? email, string? notes, DateTime createdAt, DateTime updatedAt) =>
        new() {
            Id = id,
            Name = name ?? string.Empty,
            Phone = phone ?? string.Empty,
            Email = string.IsNullOrWhiteSpace(email) ? null : email!.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes!.Trim(),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    public void Update(string name, string phone, string? email, string? notes) 
    {
        Validate(name, phone, email);

        Name = name.Trim();
        Phone = phone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string name, string phone, string? email) {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        if (phone.Length < 3)
            throw new ArgumentException("Phone is too short.", nameof(phone));

        if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
            throw new ArgumentException("Email is invalid.", nameof(email));
    }
}