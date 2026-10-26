using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.JobApplications
{
    public class DeleteApplication
    {
        public sealed record Request(Guid ApplicationId);

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(a => a.ApplicationId)
                    .NotEmpty();
            }
        }

        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapDelete("/applications/{applicationId:guid}", HandleAsync)
                    .WithName("DeleteJobApplication")
                    .WithSummary("Delete job application record")
                    .WithDescription("Deletes job application record")
                    .WithTags("JobApplications")
                    .WithValidation<Request>();
            }
        }

        private static async Task<IResult> HandleAsync(
            [AsParameters] Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var deletedEntities = await context.JobApplications
                .Where(a => a.Id == request.ApplicationId)
                .ExecuteDeleteAsync(ct);

            return Results.NoContent();
        }
    }
}
