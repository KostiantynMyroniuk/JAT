using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Domain.Entities;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Tags
{
    public static class CreateTag
    {
        public sealed record Request(string Name);
        public sealed record Response(Guid Id);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(t => t.Name)
                    .NotEmpty()
                    .MaximumLength(50);
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPost("/tags", HandleAsync)
                    .WithName("CreateTag")
                    .WithSummary("Create tag")
                    .WithTags("Tags")
                    .WithValidation<Request>()
                    .Produces<Response>(StatusCodes.Status201Created)
                    .ProducesProblem(StatusCodes.Status409Conflict);
            }
        }

        private static async Task<IResult> HandleAsync(
            Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var tag = Tag.Create(request.Name);

            context.Tags.Add(tag);

            try
            {
                await context.SaveChangesAsync(ct);
            } 
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return TypedResults.Conflict<ProblemDetails>(new() {
                    Detail = $"Tag with name: {tag.Name} already exists" });
            }

            return TypedResults.CreatedAtRoute(
                new Response(tag.Id),
                "GetTag",
                new { tag = tag.Id });
        }
    }
}
