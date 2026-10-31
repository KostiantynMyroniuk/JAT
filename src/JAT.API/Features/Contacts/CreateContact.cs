using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Domain.Entities;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Contacts
{
    public static class CreateContact
    {
        public sealed record Request(
            Guid JobApplicationId,
            string Name,
            string Email,
            string? LinkedIn);

        public sealed record Response(Guid Id);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(c => c.JobApplicationId)
                    .NotEmpty();

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
                app.MapPost("/contacts", HandleAsync)
                    .WithName("CreateContact")
                    .WithSummary("Create contact record")
                    .WithDescription("Creates contact record for a job application")
                    .WithTags("Contacts")
                    .WithValidation<Request>()
                    .Produces<Response>(StatusCodes.Status201Created)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .ProducesProblem(StatusCodes.Status409Conflict);
            }
        }

        private static async Task<IResult> HandleAsync(
            Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var applicationExists = await context.JobApplications
                .AnyAsync(a => a.Id == request.JobApplicationId, ct);

            if (!applicationExists)
            {
                return TypedResults.NotFound<ProblemDetails>(new()
                {
                    Detail = $"Application: {request.JobApplicationId} not found"
                });
            }

            var contactExists = await context.Contacts
                .AnyAsync(c => c.JobApplicationId == request.JobApplicationId, ct);

            if (contactExists)
            {
                return TypedResults.Conflict<ProblemDetails>(new()
                {
                    Detail = $"Contact for application: {request.JobApplicationId} already exists"
                });
            }

            var contact = new Contact
            {
                JobApplicationId = request.JobApplicationId,
                Name = request.Name,
                Email = request.Email,
                LinkedIn = request.LinkedIn
            };

            context.Contacts.Add(contact);

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return TypedResults.Conflict<ProblemDetails>(new()
                {
                    Detail = $"Contact for application: {request.JobApplicationId} already exists"
                });
            }

            return TypedResults.CreatedAtRoute(
                new Response(contact.Id),
                "GetContact",
                new { contactId = contact.Id });
        }
    }
}
