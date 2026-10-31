namespace JAT.API.Features.Contacts.Dtos
{
    public sealed record ContactDto(
        Guid Id,
        Guid JobApplicationId,
        string Name,
        string Email,
        string? LinkedIn);
}
