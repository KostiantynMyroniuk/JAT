using JAT.API.Domain.Enums;

namespace JAT.API.Features.Interviews.Dtos;

public sealed record InterviewDto(
    Guid Id,
    Guid JobApplicationId,
    InterviewType InterviewType,
    DateTimeOffset ScheduledAt,
    string? Location,
    string? Notes);
