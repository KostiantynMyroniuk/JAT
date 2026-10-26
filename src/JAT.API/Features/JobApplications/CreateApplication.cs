using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;

namespace JAT.API.Features.JobApplications
{
    public class CreateApplication
    {
        public sealed record Request(
            string Title,
            string? Notes,
            JobApplicationStatus Status,
            string? Url,
            DateTimeOffset AppliedAt,
            decimal? SalaryMin,
            decimal? SalaryMax);

        public sealed record Response(Guid Id);

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
                app.MapPost("/applications", HandleAsync)
                    .WithName("CreateJobApplication")
                    .WithSummary("Create job application record")
                    .WithDescription("Creates job application record")
                    .WithTags("JobApplications")
                    .WithValidation<Request>()
                    .Produces<Response>();
            }
        }

        private static async Task<IResult> HandleAsync(
            Request request,
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

            return TypedResults.Created(
                $"api/applications/{application.Id}",
                new Response(application.Id));
        }
    }
}
