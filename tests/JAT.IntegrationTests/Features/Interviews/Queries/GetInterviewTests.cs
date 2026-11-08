using System.Net;
using System.Net.Http.Json;
using JAT.API.Common.Models;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Features.Interviews.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JAT.IntegrationTests.Features.Interviews.Queries;

public class GetInterviewTests(
    CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task GetInterview_WhenExists_ReturnsOkWithData()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var application = new JobApplication
        {
            Title = "Senior C# Developer",
            Status = JobApplicationStatus.Applied
        };
        context.JobApplications.Add(application);

        var interview = new Interview
        {
            JobApplicationId = application.Id,
            InterviewType = InterviewType.Technical,
            ScheduledAt = DateTimeOffset.UtcNow.AddDays(1),
            Location = "Teams",
            Notes = "Live coding"
        };
        context.Interviews.Add(interview);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"/api/v1/interviews/{interview.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<InterviewDto>();
        Assert.NotNull(dto);
        Assert.Equal(interview.Id, dto.Id);
        Assert.Equal(application.Id, dto.JobApplicationId);
        Assert.Equal(InterviewType.Technical, dto.InterviewType);
        Assert.Equal("Teams", dto.Location);
    }

    [Fact]
    public async Task GetInterview_WhenDoesNotExist_ReturnsNotFound()
    {
        var response = await _httpClient.GetAsync($"/api/v1/interviews/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInterviewsPaginated_ReturnsPaginatedList()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var application = new JobApplication
        {
            Title = "Fullstack Engineer",
            Status = JobApplicationStatus.Applied
        };
        context.JobApplications.Add(application);

        var interview = new Interview
        {
            JobApplicationId = application.Id,
            InterviewType = InterviewType.HrScreening,
            ScheduledAt = DateTimeOffset.UtcNow.AddDays(3),
            Location = "Zoom"
        };
        context.Interviews.Add(interview);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync("/api/v1/interviews?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<InterviewDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetInterviewsPaginated_FilteredByApplication_ReturnsFilteredResults()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var app1 = new JobApplication { Title = "App 1", Status = JobApplicationStatus.Applied };
        var app2 = new JobApplication { Title = "App 2", Status = JobApplicationStatus.Applied };
        context.JobApplications.AddRange(app1, app2);

        var interview1 = new Interview
        {
            JobApplicationId = app1.Id,
            InterviewType = InterviewType.Technical,
            ScheduledAt = DateTimeOffset.UtcNow.AddDays(1)
        };
        var interview2 = new Interview
        {
            JobApplicationId = app2.Id,
            InterviewType = InterviewType.Final,
            ScheduledAt = DateTimeOffset.UtcNow.AddDays(2)
        };
        context.Interviews.AddRange(interview1, interview2);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"/api/v1/interviews?jobApplicationId={app1.Id}&pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<InterviewDto>>();
        Assert.NotNull(result);
        Assert.All(result.Items, i => Assert.Equal(app1.Id, i.JobApplicationId));
    }
}
