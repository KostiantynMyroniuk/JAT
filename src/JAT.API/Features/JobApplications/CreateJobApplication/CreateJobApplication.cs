using JAT.API.Common.Abstractions;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Infrastructure.Persistence;

namespace JAT.API.Features.JobApplications.CreateJobApplication
{
    public class CreateJobApplication
    {
        public sealed record CreateJobApplicationRequest(
            string Title,
            string? Notes,
            JobApplicationStatus Status,
            string? Url,
            DateTimeOffset AppliedAt,
            decimal SalaryMin,
            decimal SalaryMax);

        public sealed record CreateJobApplicationResponse(Guid Id);

        public sealed class CreateJobApplicationEndpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPost("api/applications", Handle)
                    .WithName("CreateJobApplication")
                    .WithSummary("Create job application record")
                    .WithDescription("Creates job application record")
                    .WithTags("JobApplications")
                    .Produces<CreateJobApplicationResponse>();
            }
        }

        private static async Task<IResult> Handle(
            CreateJobApplicationRequest request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var application = new JobApplication()
            {
                Title = request.Title,
                Notes = request.Notes,
                Status = request.Status,
                Url = request.Url,
                AppliedAt = request.AppliedAt,
                LastActivityAt = request.AppliedAt,
                SalaryMin = request.SalaryMin,
                SalaryMax = request.SalaryMax
            };

            context.JobApplications.Add(application);
            await context.SaveChangesAsync(ct);

            return Results.Created(
                $"api/applications/{application.Id}",
                new CreateJobApplicationResponse(application.Id));
        }
    }
}
