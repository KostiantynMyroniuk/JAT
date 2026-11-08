using System.Net;
using System.Net.Http.Json;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Features.Interviews.Commands.CreateInterview;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JAT.IntegrationTests.Features.Interviews.Commands;

public class CreateInterviewTests(
    CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task CreateInterview_WithValidData_ReturnsCreated()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var application = new JobApplication
        {
            Title = "Backend Engineer",
            Status = JobApplicationStatus.Applied
        };

        context.JobApplications.Add(application);
        await context.SaveChangesAsync();

        var request = new CreateInterviewRequest(
            JobApplicationId: application.Id,
            InterviewType: InterviewType.Technical,
            ScheduledAt: DateTimeOffset.UtcNow.AddDays(2),
            Location: "Google Meet",
            Notes: "System design round");

        var response = await _httpClient.PostAsJsonAsync("/api/v1/interviews", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var location = response.Headers.Location;
        Assert.NotNull(location);

        var id = Guid.Parse(location.Segments.Last());

        var createdInterview = await context.Interviews
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        Assert.NotNull(createdInterview);
        Assert.Equal(application.Id, createdInterview.JobApplicationId);
        Assert.Equal(InterviewType.Technical, createdInterview.InterviewType);
        Assert.Equal("Google Meet", createdInterview.Location);
    }

    [Fact]
    public async Task CreateInterview_WhenApplicationDoesNotExist_ReturnsNotFound()
    {
        var request = new CreateInterviewRequest(
            JobApplicationId: Guid.NewGuid(),
            InterviewType: InterviewType.HrScreening,
            ScheduledAt: DateTimeOffset.UtcNow,
            Location: "Phone call",
            Notes: "Initial screening");

        var response = await _httpClient.PostAsJsonAsync("/api/v1/interviews", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateInterview_WithInvalidData_ReturnsBadRequest()
    {
        var request = new CreateInterviewRequest(
            JobApplicationId: Guid.Empty,
            InterviewType: (InterviewType)999,
            ScheduledAt: default,
            Location: new string('a', 201),
            Notes: new string('b', 501));

        var response = await _httpClient.PostAsJsonAsync("/api/v1/interviews", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
