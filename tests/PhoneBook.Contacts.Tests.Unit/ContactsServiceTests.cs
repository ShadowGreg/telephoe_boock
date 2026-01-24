using FluentAssertions;
using NSubstitute;
using PhoneBook.Contacts.Application;
using PhoneBook.Contacts.Domain;
using Xunit;

namespace PhoneBook.Contacts.Tests.Unit;

public class ContactsServiceTests
{
    private readonly IContactsRepository _repo = Substitute.For<IContactsRepository>();

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenRepoEmpty()
    {
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Contact>());

        var svc = new ContactsService(_repo);
        var result = await svc.GetAllAsync(null, default);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSortedByName()
    {
        var a = Contact.Create("Anna", "123", null, null);
        var b = Contact.Create("Boris", "456", null, null);
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { b, a });

        var svc = new ContactsService(_repo);
        var result = await svc.GetAllAsync(null, default);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Anna");
        result[1].Name.Should().Be("Boris");
    }

    [Fact]
    public async Task GetAllAsync_WithQuery_FiltersByName()
    {
        var a = Contact.Create("Anna", "123", null, null);
        var b = Contact.Create("Boris", "456", null, null);
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { a, b });

        var svc = new ContactsService(_repo);
        var result = await svc.GetAllAsync("ann", default);

        result.Should().ContainSingle().Which.Name.Should().Be("Anna");
    }

    [Fact]
    public async Task GetAllAsync_WithQuery_FiltersByPhone()
    {
        var c = Contact.Create("Ivan", "+79001234567", null, null);
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { c });

        var svc = new ContactsService(_repo);
        var result = await svc.GetAllAsync("234", default);

        result.Should().ContainSingle().Which.Phone.Should().Be("+79001234567");
    }

    [Fact]
    public async Task GetAllAsync_WithQuery_FiltersByEmail()
    {
        var c = Contact.Create("Ivan", "123", "ivan@mail.ru", null);
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { c });

        var svc = new ContactsService(_repo);
        var result = await svc.GetAllAsync("mail", default);

        result.Should().ContainSingle().Which.Email.Should().Be("ivan@mail.ru");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Contact?)null);

        var svc = new ContactsService(_repo);
        var result = await svc.GetByIdAsync(Guid.NewGuid(), default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenFound()
    {
        var c = Contact.Create("Ivan", "+79001", "i@m.ru", "n");
        _repo.GetByIdAsync(c.Id, Arg.Any<CancellationToken>()).Returns(c);

        var svc = new ContactsService(_repo);
        var result = await svc.GetByIdAsync(c.Id, default);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Ivan");
        result.Phone.Should().Be("+79001");
        result.Email.Should().Be("i@m.ru");
        result.Notes.Should().Be("n");
    }

    [Fact]
    public async Task CreateAsync_AddsToRepoAndReturnsDto()
    {
        _repo.AddAsync(Arg.Any<Contact>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var svc = new ContactsService(_repo);
        var req = new CreateContactRequest("Ivan", "+79001234567", "i@m.ru", "note");
        var result = await svc.CreateAsync(req, default);

        result.Name.Should().Be("Ivan");
        result.Phone.Should().Be("+79001234567");
        result.Email.Should().Be("i@m.ru");
        result.Notes.Should().Be("note");
        result.Id.Should().NotBeEmpty();

        await _repo.Received(1).AddAsync(Arg.Is<Contact>(x => x.Name == "Ivan"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithInvalidData_Throws()
    {
        var svc = new ContactsService(_repo);
        var req = new CreateContactRequest("", "+79001", null, null);

        var act = () => svc.CreateAsync(req, default);

        await act.Should().ThrowAsync<ArgumentException>();
        await _repo.DidNotReceive().AddAsync(Arg.Any<Contact>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Contact?)null);

        var svc = new ContactsService(_repo);
        var req = new UpdateContactRequest("Ivan", "+79001", null, null);
        var result = await svc.UpdateAsync(Guid.NewGuid(), req, default);

        result.Should().BeNull();
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Contact>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsDto_WhenFound()
    {
        var c = Contact.Create("Ivan", "+79001", null, null);
        _repo.GetByIdAsync(c.Id, Arg.Any<CancellationToken>()).Returns(c);
        _repo.UpdateAsync(Arg.Any<Contact>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var svc = new ContactsService(_repo);
        var req = new UpdateContactRequest("Petr", "+79002", "p@m.ru", "n2");
        var result = await svc.UpdateAsync(c.Id, req, default);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Petr");
        result.Phone.Should().Be("+79002");
        result.Email.Should().Be("p@m.ru");
        result.Notes.Should().Be("n2");

        await _repo.Received(1).UpdateAsync(Arg.Is<Contact>(x => x.Name == "Petr"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        _repo.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var svc = new ContactsService(_repo);
        var result = await svc.DeleteAsync(Guid.NewGuid(), default);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        _repo.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var svc = new ContactsService(_repo);
        var result = await svc.DeleteAsync(Guid.NewGuid(), default);

        result.Should().BeTrue();
    }
}
