using System.Net;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JAT.IntegrationTests.Features.Interviews.Commands;

public class DeleteInterviewTests(
    CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task DeleteInterview_WhenExists_ReturnsNoContent()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var application = new JobApplication
        {
            Title = "DevOps Specialist",
            Status = JobApplicationStatus.Applied
        };
        context.JobApplications.Add(application);

        var interview = new Interview
        {
            JobApplicationId = application.Id,
            InterviewType = InterviewType.Technical,
            ScheduledAt = DateTimeOffset.UtcNow.AddDays(2),
            Location = "Google Meet"
        };
        context.Interviews.Add(interview);
        await context.SaveChangesAsync();

        var response = await _httpClient.DeleteAsync($"/api/v1/interviews/{interview.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var deleted = await context.Interviews
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interview.Id);

        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteInterview_WhenDoesNotExist_ReturnsNotFound()
    {
        var response = await _httpClient.DeleteAsync($"/api/v1/interviews/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
