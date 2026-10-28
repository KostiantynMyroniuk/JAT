using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Domain.Entities;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace JAT.API.Features.JobApplications
{
    public static class SetApplicationTags
    {
        public sealed record Request(Guid[] TagIds);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.TagIds)
                    .NotNull()
                    .Must(x => x.Length <= 20);

                RuleForEach(x => x.TagIds)
                    .NotEmpty();
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPut("/applications/{applicationId:guid}/tags", HandleAsync)
                    .WithName("SetApplicationTags")
                    .WithSummary("Set tags for job application")
                    .WithDescription("Sets chosen tags for job application")
                    .WithTags("JobApplications")
                    .WithValidation<Request>()
                    .Produces(StatusCodes.Status204NoContent)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .ProducesValidationProblem();
            }
        }

        private static async Task<IResult> HandleAsync(
            Guid applicationId,
            Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var application = await context.JobApplications
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct);

            if (application == null)
                return TypedResults.NotFound<ProblemDetails>(new() { 
                    Detail = $"Application: {applicationId} not found" });

            var ids = request.TagIds.Distinct().ToArray();

            var tags = await context.Tags
                .Where(t => ids.Contains(t.Id))
                .ToListAsync(ct);

            if (tags.Count != ids.Length)
            {
                var missingIds = ids.Except(tags.Select(t => t.Id));

                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["tagIds"] = [$"Tags not found: {string.Join(", ", missingIds)}"]
                });
            }

            application.SetTags(tags);
            await context.SaveChangesAsync(ct);

            return TypedResults.NoContent();
        }
    }
}
