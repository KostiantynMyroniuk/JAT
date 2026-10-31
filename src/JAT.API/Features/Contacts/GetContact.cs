using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Features.Contacts.Dtos;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Contacts
{
    public static class GetContact
    {
        public sealed record Request(Guid ContactId);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(c => c.ContactId).NotEmpty();
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapGet("/contacts/{contactId:guid}", HandleAsync)
                    .WithName("GetContact")
                    .WithSummary("Get contact by id")
                    .WithDescription("Returns the contact details")
                    .WithTags("Contacts")
                    .WithValidation<Request>()
                    .Produces<ContactDto>(StatusCodes.Status200OK)
                    .ProducesProblem(StatusCodes.Status404NotFound);
            }
        }

        private static async Task<IResult> HandleAsync(
            [AsParameters] Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var response = await context.Contacts
                .AsNoTracking()
                .Where(c => c.Id == request.ContactId)
                .Select(c => new ContactDto(
                    c.Id,
                    c.JobApplicationId,
                    c.Name,
                    c.Email,
                    c.LinkedIn))
                .FirstOrDefaultAsync(ct);

            if (response == null)
            {
                return TypedResults.NotFound<ProblemDetails>(new()
                {
                    Detail = $"Contact: {request.ContactId} not found"
                });
            }

            return TypedResults.Ok(response);
        }
    }
}
