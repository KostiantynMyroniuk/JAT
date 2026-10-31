using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace JAT.API.Features.Contacts
{
    public static class UpdateContact
    {
        public sealed record Request(
            string Name,
            string Email,
            string? LinkedIn);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(c => c.Name)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(c => c.Email)
                    .NotEmpty()
                    .MaximumLength(100)
                    .EmailAddress();

                RuleFor(c => c.LinkedIn)
                    .MaximumLength(200)
                    .Must(BeUrl)
                    .When(x => !string.IsNullOrWhiteSpace(x.LinkedIn));
            }

            private static bool BeUrl(string? url) =>
                Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPut("/contacts/{contactId:guid}", HandleAsync)
                    .WithName("UpdateContact")
                    .WithSummary("Update contact record")
                    .WithDescription("Updates contact record")
                    .WithTags("Contacts")
                    .WithValidation<Request>()
                    .Produces(StatusCodes.Status204NoContent)
                    .ProducesProblem(StatusCodes.Status404NotFound);
            }
        }

        private static async Task<IResult> HandleAsync(
            Guid contactId,
            Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var contact = await context.Contacts
                .FindAsync([contactId], ct);

            if (contact == null)
            {
                return TypedResults.NotFound<ProblemDetails>(new()
                {
                    Detail = $"Contact: {contactId} not found"
                });
            }

            context.Entry(contact).CurrentValues.SetValues(request);
            await context.SaveChangesAsync(ct);

            return TypedResults.NoContent();
        }
    }
}
