using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Contacts
{
    public static class DeleteContact
    {
        public sealed record Request(Guid ContactId);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(c => c.ContactId)
                    .NotEmpty();
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapDelete("/contacts/{contactId:guid}", HandleAsync)
                    .WithName("DeleteContact")
                    .WithSummary("Delete contact record")
                    .WithDescription("Deletes contact record")
                    .WithTags("Contacts")
                    .WithValidation<Request>()
                    .Produces(StatusCodes.Status204NoContent);
            }
        }

        private static async Task<IResult> HandleAsync(
            [AsParameters] Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var deletedEntities = await context.Contacts
                .Where(c => c.Id == request.ContactId)
                .ExecuteDeleteAsync(ct);

            return TypedResults.NoContent();
        }
    }
}
