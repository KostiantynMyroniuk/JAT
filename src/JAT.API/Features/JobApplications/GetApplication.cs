using Asp.Versioning;
using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Features.JobApplications.Dtos;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.JobApplications
{
    public static class GetApplication
    {
        public sealed record Request(Guid ApplicationId);

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
                    .Produces<JobApplicationDto>(StatusCodes.Status200OK)
                    .ProducesProblem(StatusCodes.Status404NotFound);
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
                .Select(a => new JobApplicationDto(
                    a.Id,
                    a.Title,
                    a.Notes,
                    a.Status,
                    a.Url,
                    a.AppliedAt,
                    a.SalaryMin,
                    a.SalaryMax,
                    a.Tags
                        .OrderBy(t => t.Name)
                        .Select(t => new JobApplicationTagDto(t.Id, t.Name))
                        .ToList()))
                .FirstOrDefaultAsync(ct);

            if (response == null)
                return TypedResults.NotFound<ProblemDetails>(new() {
                    Detail = $"Application: {request.ApplicationId} not found" });

            return TypedResults.Ok(response);
        }
    }
}
