using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Features.Contacts.Dtos;
using JAT.API.Filters;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Contacts
{
    public static class GetContactsPaginated
    {
        public sealed record Request(
            Guid? JobApplicationId = null,
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

                RuleFor(r => r.JobApplicationId)
                    .NotEmpty()
                    .When(r => r.JobApplicationId.HasValue);
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapGet("/contacts", HandleAsync)
                    .WithName("GetContactsPaginated")
                    .WithSummary("Get contacts records by pages")
                    .WithDescription("Gets contacts records by pages with optional application filter")
                    .WithTags("Contacts")
                    .WithValidation<Request>()
                    .Produces<PaginatedList<ContactDto>>(StatusCodes.Status200OK);
            }
        }

        private static async Task<IResult> HandleAsync(
            [AsParameters] Request request,
            ApplicationDbContext context,
            CancellationToken ct)
        {
            var query = context.Contacts.AsNoTracking();

            if (request.JobApplicationId.HasValue)
            {
                query = query.Where(c => c.JobApplicationId == request.JobApplicationId.Value);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new ContactDto(
                    c.Id,
                    c.JobApplicationId,
                    c.Name,
                    c.Email,
                    c.LinkedIn))
                .ToListAsync(ct);

            return TypedResults.Ok(
                new PaginatedList<ContactDto>(items, request.PageNumber, request.PageSize, totalCount));
        }
    }
}
