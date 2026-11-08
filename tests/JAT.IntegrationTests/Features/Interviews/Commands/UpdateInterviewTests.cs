using System.Net;
using System.Net.Http.Json;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Features.Interviews.Commands.UpdateInterview;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JAT.IntegrationTests.Features.Interviews.Commands;

public class UpdateInterviewTests(
    CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task UpdateInterview_WithValidData_ReturnsNoContent()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var application = new JobApplication
        {
            Title = "Staff Engineer",
            Status = JobApplicationStatus.Applied
        };
        context.JobApplications.Add(application);

        var interview = new Interview
        {
            JobApplicationId = application.Id,
            InterviewType = InterviewType.HrScreening,
            ScheduledAt = DateTimeOffset.UtcNow.AddDays(1),
            Location = "Phone",
            Notes = "Introductory call"
        };
        context.Interviews.Add(interview);
        await context.SaveChangesAsync();

        var updatedScheduledAt = DateTimeOffset.UtcNow.AddDays(4);
        var request = new UpdateInterviewRequest(
            InterviewType: InterviewType.Final,
            ScheduledAt: updatedScheduledAt,
            Location: "On-site office",
            Notes: "Executive round");

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/interviews/{interview.Id}", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await context.Interviews
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interview.Id);

        Assert.NotNull(updated);
        Assert.Equal(InterviewType.Final, updated.InterviewType);
        Assert.Equal("On-site office", updated.Location);
        Assert.Equal("Executive round", updated.Notes);
    }

    [Fact]
    public async Task UpdateInterview_WhenDoesNotExist_ReturnsNotFound()
    {
        var request = new UpdateInterviewRequest(
            InterviewType: InterviewType.Technical,
            ScheduledAt: DateTimeOffset.UtcNow.AddDays(1),
            Location: "Zoom",
            Notes: null);

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/interviews/{Guid.NewGuid()}", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
