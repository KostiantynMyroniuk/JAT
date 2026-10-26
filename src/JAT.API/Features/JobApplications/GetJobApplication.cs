using Asp.Versioning;
using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.JobApplications
{
    public class GetJobApplication
    {
        public sealed record Request(Guid ApplicationId);

        public sealed record TagResponse(Guid Id, string Name);
        public sealed record Response(
            Guid Id,
            string Title,
            string? Notes,
            JobApplicationStatus Status,
            string? Url,
            DateTimeOffset AppliedAt,
            DateTimeOffset LastActivityAt,
            decimal? SalaryMin,
            decimal? SalaryMax,
            IEnumerable<TagResponse> Tags);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(a => a.ApplicationId).NotEmpty();
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapGet("/applications/{applicationId:guid}", HandleAsync)
                    .WithName("GetJobApplication")
                    .WithSummary("Get job application by id")
                    .WithDescription("Returns the job application with its tags")
                    .WithTags("JobApplications")
                    .WithValidation<Request>()
                    .Produces<Response>()
                    .ProducesProblem(StatusCodes.Status404NotFound)
            }
        }

        private static async Task<IResult> HandleAsync(
            [AsParameters] Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var response = await context.JobApplications
                .AsNoTracking()
                .Where(a => a.Id == request.ApplicationId)
                .Select(a => new Response(
                    a.Id,
                    a.Title,
                    a.Notes,
                    a.Status,
                    a.Url,
                    a.AppliedAt,
                    a.LastActivityAt,
                    a.SalaryMin,
                    a.SalaryMax,
                    a.Tags
                        .OrderBy(t => t.Name)
                        .Select(t => new TagResponse(t.Id, t.Name))))
                .FirstOrDefaultAsync(ct);

            if (response == null)
                return Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Job application not found");

            return Results.Ok(response);
        }
    }
}
