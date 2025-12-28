using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PhoneBook.Contacts.Application;

namespace PhoneBook.Api;
public static class ContactsEndpoints {
    public static IEndpointRouteBuilder MapContactsModule(
        this IEndpointRouteBuilder app) {
        var group = app.MapGroup("/api/contacts")
                       .WithTags("Contacts")
                       .WithOpenApi();

        // GET /api/contacts?q=
        group.MapGet(
                  "/",
                  async Task<Ok<IReadOnlyList<ContactDto>>> (
                      string? q,
                      [FromServices] ContactsService service,
                      CancellationToken ct) =>
                      {
                          var items = await service.GetAllAsync(q, ct);
                          return TypedResults.Ok(items);
                      })
             .WithName("GetContacts")
             .WithSummary("Получить список контактов")
             .WithDescription("Возвращает список контактов с возможностью поиска по имени, телефону или email")
             .Produces<IReadOnlyList<ContactDto>>(StatusCodes.Status200OK);

        // GET /api/contacts/{id}
        group.MapGet(
                  "/{id:guid}",
                  async Task<Results<Ok<ContactDto>, NotFound>> (
                      Guid id,
                      [FromServices] ContactsService service,
                      CancellationToken ct) =>
                      {
                          var item = await service.GetByIdAsync(id, ct);

                          return item is null
                              ? TypedResults.NotFound()
                              : TypedResults.Ok(item);
                      })
             .WithName("GetContactById")
             .WithSummary("Получить контакт по Id")
             .Produces<ContactDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound);

        // POST /api/contacts
        group.MapPost(
                  "/",
                  async Task<Results<Created<ContactDto>, BadRequest<ProblemDetails>>> (
                      CreateContactRequest request,
                      [FromServices] ContactsService service,
                      CancellationToken ct) =>
                      {
                          try {
                              var created = await service.CreateAsync(request, ct);

                              return TypedResults.Created(
                                  $"/api/contacts/{created.Id}",
                                  created);
                          }
                          catch (ArgumentException ex) {
                              return TypedResults.BadRequest(
                                  new ProblemDetails {
                                                         Title = "Validation error",
                                                         Detail = ex.Message,
                                                         Status = StatusCodes.Status400BadRequest
                                                     });
                          }
                      })
             .WithName("CreateContact")
             .WithSummary("Создать контакт")
             .Produces<ContactDto>(StatusCodes.Status201Created)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        // PUT /api/contacts/{id}
        group.MapPut(
                  "/{id:guid}",
                  async Task<Results<Ok<ContactDto>, NotFound, BadRequest<ProblemDetails>>> (
                      Guid id,
                      UpdateContactRequest request,
                      [FromServices] ContactsService service,
                      CancellationToken ct) =>
                      {
                          try {
                              var updated = await service.UpdateAsync(id, request, ct);

                              return updated is null
                                  ? TypedResults.NotFound()
                                  : TypedResults.Ok(updated);
                          }
                          catch (ArgumentException ex) {
                              return TypedResults.BadRequest(
                                  new ProblemDetails {
                                                         Title = "Validation error",
                                                         Detail = ex.Message,
                                                         Status = StatusCodes.Status400BadRequest
                                                     });
                          }
                      })
             .WithName("UpdateContact")
             .WithSummary("Обновить контакт")
             .Produces<ContactDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        // DELETE /api/contacts/{id}
        group.MapDelete(
                  "/{id:guid}",
                  async Task<Results<NoContent, NotFound>> (
                      Guid id,
                      [FromServices] ContactsService service,
                      CancellationToken ct) =>
                      {
                          var deleted = await service.DeleteAsync(id, ct);

                          return deleted
                              ? TypedResults.NoContent()
                              : TypedResults.NotFound();
                      })
             .WithName("DeleteContact")
             .WithSummary("Удалить контакт")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}