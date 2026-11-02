using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Features.Contacts.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Contacts.Queries.GetContact;

public sealed record GetContactQuery(Guid ContactId);

public sealed class GetContactHandler(ApplicationDbContext context)
    : IHandler<GetContactQuery, Result<ContactDto>>
{
    public async Task<Result<ContactDto>> HandleAsync(
        GetContactQuery query,
        CancellationToken ct = default)
    {
        var response = await context.Contacts
            .AsNoTracking()
            .Where(c => c.Id == query.ContactId)
            .Select(c => new ContactDto(
                c.Id,
                c.JobApplicationId,
                c.Name,
                c.Email,
                c.LinkedIn))
            .FirstOrDefaultAsync(ct);

        if (response is null)
        {
            return Result.Failure<ContactDto>(
                Error.NotFound($"Contact: {query.ContactId} not found"));
        }

        return Result.Success(response);
    }
}
