using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Features.Tags.Dtos;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Tags
{
    public static class GetTagsPaginated
    {
        public sealed record Request(
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
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapGet("/tags", HandleAsync)
                    .WithName("GetTagsPaginated")
                    .WithSummary("Get tags by pages")
                    .WithTags("Tags")
                    .WithValidation<Request>()
                    .Produces<PaginatedList<TagDto>>(StatusCodes.Status200OK);
            }
        }

        private static async Task<IResult> HandleAsync(
            [AsParameters] Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var query = context.Tags.AsNoTracking();

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(t => t.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TagDto(
                    t.Id, 
                    t.Name))
                .ToListAsync(ct);

            return TypedResults.Ok(
                new PaginatedList<TagDto>(items, request.PageNumber, request.PageSize, totalCount));
        }
    }
}
