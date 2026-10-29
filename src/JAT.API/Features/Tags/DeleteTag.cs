using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Tags
{
    public static class DeleteTag
    {
        public sealed record Request(Guid TagId);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(t => t.TagId)
                    .NotEmpty();
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapDelete("/tags/{tagId:guid}", HandleAsync)
                    .WithName("DeleteTag")
                    .WithSummary("Delete tag")
                    .WithTags("Tags")
                    .WithValidation<Request>()
                    .Produces(StatusCodes.Status204NoContent);
            }
        }

        private static async Task<IResult> HandleAsync(
            [AsParameters] Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var deletedTags = await context.Tags
                .Where(t => t.Id == request.TagId)
                .ExecuteDeleteAsync(ct);

            return TypedResults.NoContent();
        }
    }
}
