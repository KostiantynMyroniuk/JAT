using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Features.JobApplications.Dtos;

namespace JAT.API.Features.JobApplications.Queries.GetApplication;

public sealed class GetApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/applications/{applicationId:guid}", HandleAsync)
            .WithName("GetJobApplication")
            .WithSummary("Get job application by id")
            .WithDescription("Returns the job application with its tags")
            .WithTags("JobApplications")
            .Produces<JobApplicationDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(
        Guid applicationId,
        IHandler<GetApplicationQuery, Result<JobApplicationDto>> handler,
        CancellationToken ct)
    {
        var query = new GetApplicationQuery(applicationId);
        var result = await handler.HandleAsync(query, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}
