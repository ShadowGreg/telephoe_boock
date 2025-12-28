namespace PhoneBook.Contacts.Infrastructure;

public sealed class ContactsFileStoreOptions
{
    public string FilePath { get; set; } = "./data/contacts.json";
}