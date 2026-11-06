using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Extensions;

namespace JAT.API.Features.JobApplications.Commands.UpdateApplication;

public sealed record UpdateApplicationRequest(
    string Title,
    string? Notes,
    JobApplicationStatus Status,
    string? Url,
    DateTimeOffset? AppliedAt,
    decimal? SalaryMin,
    decimal? SalaryMax);

public sealed class UpdateApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/applications/{applicationId:guid}", HandleAsync)
            .WithName("UpdateJobApplication")
            .WithSummary("Update job application record")
            .WithDescription("Updates job application record")
            .WithTags("JobApplications")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> HandleAsync(
        Guid applicationId,
        UpdateApplicationRequest request,
        IHandler<UpdateApplicationCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new UpdateApplicationCommand(
            applicationId,
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

        return TypedResults.NoContent();
    }
}
