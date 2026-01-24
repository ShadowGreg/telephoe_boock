using FluentAssertions;
using PhoneBook.Contacts.Domain;
using Xunit;

namespace PhoneBook.Contacts.Tests.Unit;

public class ContactTests
{
    [Fact]
    public void Create_WithValidData_ReturnsContact()
    {
        var c = Contact.Create("Ivan", "+79001234567", "ivan@mail.ru", "note");

        c.Id.Should().NotBeEmpty();
        c.Name.Should().Be("Ivan");
        c.Phone.Should().Be("+79001234567");
        c.Email.Should().Be("ivan@mail.ru");
        c.Notes.Should().Be("note");
        c.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        c.UpdatedAt.Should().Be(c.CreatedAt);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var c = Contact.Create("  Ivan  ", "  123  ", " a@b ", " n ");

        c.Name.Should().Be("Ivan");
        c.Phone.Should().Be("123");
        c.Email.Should().Be("a@b");
        c.Notes.Should().Be("n");
    }

    [Fact]
    public void Create_WithNullEmailAndNotes_StoresNull()
    {
        var c = Contact.Create("Ivan", "123", null, null);

        c.Email.Should().BeNull();
        c.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_Throws(string? name)
    {
        var act = () => Contact.Create(name!, "+79001234567", null, null);

        act.Should().Throw<ArgumentException>().WithMessage("*Name is required*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPhone_Throws(string? phone)
    {
        var act = () => Contact.Create("Ivan", phone!, null, null);

        act.Should().Throw<ArgumentException>().WithMessage("*Phone is required*");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    public void Create_WithShortPhone_Throws(string phone)
    {
        var act = () => Contact.Create("Ivan", phone, null, null);

        act.Should().Throw<ArgumentException>().WithMessage("*Phone is too short*");
    }

    [Theory]
    [InlineData("bad")]
    [InlineData("no-at-sign")]
    public void Create_WithInvalidEmail_Throws(string email)
    {
        var act = () => Contact.Create("Ivan", "+79001234567", email, null);

        act.Should().Throw<ArgumentException>().WithMessage("*Email is invalid*");
    }

    [Fact]
    public void Create_WithValidEmail_Succeeds()
    {
        var c = Contact.Create("Ivan", "+79001234567", "a@b.co", null);

        c.Email.Should().Be("a@b.co");
    }

    [Fact]
    public void FromPersistence_ReturnsContactWithGivenData()
    {
        var id = Guid.NewGuid();
        var created = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updated = new DateTime(2025, 1, 2, 11, 0, 0, DateTimeKind.Utc);

        var c = Contact.FromPersistence(id, "Ivan", "+79001", "i@mail.ru", "x", created, updated);

        c.Id.Should().Be(id);
        c.Name.Should().Be("Ivan");
        c.Phone.Should().Be("+79001");
        c.Email.Should().Be("i@mail.ru");
        c.Notes.Should().Be("x");
        c.CreatedAt.Should().Be(created);
        c.UpdatedAt.Should().Be(updated);
    }

    [Fact]
    public void FromPersistence_WithNullName_UsesEmptyString()
    {
        var c = Contact.FromPersistence(Guid.NewGuid(), null!, "1", null, null, DateTime.UtcNow, DateTime.UtcNow);

        c.Name.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithValidData_UpdatesProperties()
    {
        var c = Contact.Create("Ivan", "+79001", "i@m.ru", "n1");
        var before = c.UpdatedAt;

        c.Update("Petr", "+79002", "p@m.ru", "n2");

        c.Name.Should().Be("Petr");
        c.Phone.Should().Be("+79002");
        c.Email.Should().Be("p@m.ru");
        c.Notes.Should().Be("n2");
        c.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Update_WithInvalidName_Throws()
    {
        var c = Contact.Create("Ivan", "+79001234567", null, null);

        var act = () => c.Update("", "+79001", null, null);

        act.Should().Throw<ArgumentException>().WithMessage("*Name is required*");
    }

    [Fact]
    public void Update_WithInvalidEmail_Throws()
    {
        var c = Contact.Create("Ivan", "+79001234567", null, null);

        var act = () => c.Update("Ivan", "+79001", "bad", null);

        act.Should().Throw<ArgumentException>().WithMessage("*Email is invalid*");
    }
}
