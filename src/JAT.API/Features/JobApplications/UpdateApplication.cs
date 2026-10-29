using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Domain.Enums;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace JAT.API.Features.JobApplications
{
    public static class UpdateApplication
    {
        public sealed record Request(
            string Title,
            string? Notes,
            JobApplicationStatus Status,
            string? Url,
            DateTimeOffset AppliedAt,
            decimal? SalaryMin,
            decimal? SalaryMax);

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator(TimeProvider timeProvider)
            {
                RuleFor(a => a.Title).NotEmpty().MaximumLength(100);
                RuleFor(a => a.Notes).MaximumLength(500);
                RuleFor(a => a.Status).IsInEnum();

                RuleFor(a => a.Url)
                    .MaximumLength(200)
                    .Must(BeUrl)
                    .When(x => !string.IsNullOrWhiteSpace(x.Url));

                RuleFor(a => a.AppliedAt)
                    .NotEmpty()
                    .LessThanOrEqualTo(_ => timeProvider.GetUtcNow().AddDays(1));

                RuleFor(a => a.SalaryMin)
                    .GreaterThanOrEqualTo(0);

                RuleFor(a => a.SalaryMax)
                    .GreaterThanOrEqualTo(a => a.SalaryMin);
            }

            private static bool BeUrl(string? url) =>
                Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPut("/applications/{applicationId:guid}", HandleAsync)
                    .WithName("UpdateJobApplication")
                    .WithSummary("Update job application record")
                    .WithDescription("Updates job application record")
                    .WithTags("JobApplications")
                    .WithValidation<Request>()
                    .Produces(StatusCodes.Status204NoContent)
                    .ProducesProblem(StatusCodes.Status404NotFound);
            }
        }

        private static async Task<IResult> HandleAsync(
            Guid applicationId,
            Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var application = await context.JobApplications
                .FindAsync([applicationId], ct);

            if (application == null)
                return TypedResults.NotFound<ProblemDetails>(new() {
                    Detail = $"Application: {applicationId} not found" });

            context.Entry(application).CurrentValues.SetValues(request);
            await context.SaveChangesAsync(ct);

            return TypedResults.NoContent();
        }
    }
}
