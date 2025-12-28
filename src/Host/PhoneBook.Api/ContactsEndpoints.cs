using PhoneBook.Contacts.Application;

namespace PhoneBook.Api;
public static class ContactsEndpoints
{
    public static IEndpointRouteBuilder MapContactsModule(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contacts")
                       .WithTags("Contacts");

        group.MapGet("/", async (string? q, ContactsService service, CancellationToken ct) =>
            {
                var items = await service.GetAllAsync(q, ct);
                return Results.Ok(items);
            });

        group.MapGet("/{id:guid}", async (Guid id, ContactsService service, CancellationToken ct) =>
            {
                var item = await service.GetByIdAsync(id, ct);
                return item is null ? Results.NotFound() : Results.Ok(item);
            });

        group.MapPost("/", async (CreateContactRequest req, ContactsService service, CancellationToken ct) =>
            {
                try
                {
                    var created = await service.CreateAsync(req, ct);
                    return Results.Created($"/api/contacts/{created.Id}", created);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

        group.MapPut("/{id:guid}", async (Guid id, UpdateContactRequest req, ContactsService service, CancellationToken ct) =>
            {
                try
                {
                    var updated = await service.UpdateAsync(id, req, ct);
                    return updated is null ? Results.NotFound() : Results.Ok(updated);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

        group.MapDelete("/{id:guid}", async (Guid id, ContactsService service, CancellationToken ct) =>
            {
                var deleted = await service.DeleteAsync(id, ct);
                return deleted ? Results.NoContent() : Results.NotFound();
            });

        return app;
    }
}