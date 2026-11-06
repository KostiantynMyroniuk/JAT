using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Extensions;

namespace JAT.API.Features.JobApplications.Commands.CreateApplication;

public sealed record CreateApplicationRequest(
    string Title,
    string? Notes,
    JobApplicationStatus Status,
    string? Url,
    DateTimeOffset? AppliedAt,
    decimal? SalaryMin,
    decimal? SalaryMax);

public sealed class CreateApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/applications", HandleAsync)
            .WithName("CreateJobApplication")
            .WithSummary("Create job application record")
            .WithDescription("Creates job application record")
            .WithTags("JobApplications")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> HandleAsync(
        CreateApplicationRequest request,
        IHandler<CreateApplicationCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
        var command = new CreateApplicationCommand(
            request.Title,
            request.Notes,
            request.Status,
            request.Url,
            request.AppliedAt,
            request.SalaryMin,
            request.SalaryMax);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.CreatedAtRoute(
            result.Value,
            "GetJobApplication",
            new { applicationId = result.Value });
    }
}
