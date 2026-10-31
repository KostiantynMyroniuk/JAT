using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Features.JobApplications.Dtos;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.JobApplications
{
    public static class GetApplicationsPaginated
    {
        public sealed record Request(
            JobApplicationStatus? Status,
            int PageNumber = 1,
            int PageSize = 10);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(r => r.PageNumber)
                    .InclusiveBetween(1, 100_000);

                RuleFor(r => r.PageSize)
                    .InclusiveBetween(1, 100);

                RuleFor(r => r.Status)
                    .IsInEnum();
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapGet("/applications", HandleAsync)
                    .WithName("GetApplicationsPaginated")
                    .WithSummary("Get applications records by pages")
                    .WithDescription("Gets applications records with its tags by pages")
                    .WithTags("JobApplications")
                    .WithValidation<Request>()
                    .Produces<PaginatedList<JobApplicationDto>>(StatusCodes.Status200OK);

            }
        }

        private static async Task<IResult> HandleAsync(
            [AsParameters] Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var query = context.JobApplications.AsNoTracking();

            if (request.Status.HasValue)
            {
                query = query.Where(a => a.Status == request.Status);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(i => i.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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
                .ToListAsync(ct);

            return TypedResults.Ok(
                new PaginatedList<JobApplicationDto>(items, request.PageNumber, request.PageSize, totalCount));
        }
    }
}
