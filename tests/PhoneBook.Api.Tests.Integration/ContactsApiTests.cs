using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PhoneBook.Contacts.Domain;
using Xunit;

namespace PhoneBook.Api.Tests.Integration;

public class ContactsApiTests : IClassFixture<ApiFactory>, IDisposable
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpt = new() { PropertyNameCaseInsensitive = true };

    public ContactsApiTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        EnsureEmptyStorage();
    }

    public void Dispose() => _client.Dispose();

    private void EnsureEmptyStorage()
    {
        if (File.Exists(_factory.TempFilePath))
            File.Delete(_factory.TempFilePath);
    }

    [Fact]
    public async Task GetContacts_WhenEmpty_Returns200AndEmptyList()
    {
        var res = await _client.GetAsync("/api/contacts");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await res.Content.ReadFromJsonAsync<List<ContactDto>>(JsonOpt);
        list.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task PostContact_WithValidData_Returns201AndCreated()
    {
        var req = new CreateContactRequest("Integration User", "+79001234567", "it@test.ru", "note");

        var res = await _client.PostAsJsonAsync("/api/contacts", req);

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        res.Headers.Location?.ToString().Should().Contain("/api/contacts/");
        var created = await res.Content.ReadFromJsonAsync<ContactDto>(JsonOpt);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Integration User");
        created.Phone.Should().Be("+79001234567");
        created.Email.Should().Be("it@test.ru");
        created.Notes.Should().Be("note");
        created.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostContact_WithInvalidData_Returns400()
    {
        var req = new CreateContactRequest("", "+79001", null, null);

        var res = await _client.PostAsJsonAsync("/api/contacts", req);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetContacts_AfterPost_ReturnsCreatedContact()
    {
        var req = new CreateContactRequest("List Test", "+79001112233", "list@test.ru", null);
        await _client.PostAsJsonAsync("/api/contacts", req);

        var res = await _client.GetAsync("/api/contacts");

        res.EnsureSuccessStatusCode();
        var list = await res.Content.ReadFromJsonAsync<List<ContactDto>>(JsonOpt);
        list.Should().ContainSingle().Which.Name.Should().Be("List Test");
    }

    [Fact]
    public async Task GetContactById_WhenExists_Returns200()
    {
        var req = new CreateContactRequest("ById Test", "+79002223344", "byid@test.ru", null);
        var post = await _client.PostAsJsonAsync("/api/contacts", req);
        var created = await post.Content.ReadFromJsonAsync<ContactDto>(JsonOpt);

        var res = await _client.GetAsync($"/api/contacts/{created!.Id}");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var c = await res.Content.ReadFromJsonAsync<ContactDto>(JsonOpt);
        c!.Name.Should().Be("ById Test");
    }

    [Fact]
    public async Task GetContactById_WhenNotExists_Returns404()
    {
        var res = await _client.GetAsync($"/api/contacts/{Guid.NewGuid()}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutContact_WhenExists_Returns200AndUpdated()
    {
        var req = new CreateContactRequest("Before", "+79003334455", null, null);
        var post = await _client.PostAsJsonAsync("/api/contacts", req);
        var created = await post.Content.ReadFromJsonAsync<ContactDto>(JsonOpt);
        var update = new UpdateContactRequest("After", "+79009998877", "after@test.ru", "updated");

        var res = await _client.PutAsJsonAsync($"/api/contacts/{created!.Id}", update);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await res.Content.ReadFromJsonAsync<ContactDto>(JsonOpt);
        updated!.Name.Should().Be("After");
        updated.Phone.Should().Be("+79009998877");
        updated.Email.Should().Be("after@test.ru");
        updated.Notes.Should().Be("updated");
    }

    [Fact]
    public async Task PutContact_WhenNotExists_Returns404()
    {
        var update = new UpdateContactRequest("X", "+79001", null, null);

        var res = await _client.PutAsJsonAsync($"/api/contacts/{Guid.NewGuid()}", update);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteContact_WhenExists_Returns204()
    {
        var req = new CreateContactRequest("ToDelete", "+79004445566", null, null);
        var post = await _client.PostAsJsonAsync("/api/contacts", req);
        var created = await post.Content.ReadFromJsonAsync<ContactDto>(JsonOpt);

        var res = await _client.DeleteAsync($"/api/contacts/{created!.Id}");

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteContact_WhenNotExists_Returns404()
    {
        var res = await _client.DeleteAsync($"/api/contacts/{Guid.NewGuid()}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetContacts_WithQuery_FiltersResults()
    {
        await _client.PostAsJsonAsync("/api/contacts", new CreateContactRequest("Alice", "+79001", "a@b.ru", null));
        await _client.PostAsJsonAsync("/api/contacts", new CreateContactRequest("Bob", "+79002", "b@b.ru", null));

        var res = await _client.GetAsync("/api/contacts?q=alice");

        res.EnsureSuccessStatusCode();
        var list = await res.Content.ReadFromJsonAsync<List<ContactDto>>(JsonOpt);
        list.Should().ContainSingle().Which.Name.Should().Be("Alice");
    }
}
