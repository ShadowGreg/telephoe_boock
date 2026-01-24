namespace PhoneBook.Contacts.Domain;

public sealed record ContactDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public sealed record CreateContactRequest(
    string Name,
    string Phone,
    string? Email,
    string? Notes
);

public sealed record UpdateContactRequest(
    string Name,
    string Phone,
    string? Email,
    string? Notes
);