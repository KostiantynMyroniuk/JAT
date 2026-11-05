using System.Net;
using System.Net.Http.Json;
using JAT.API.Domain.Enums;
using JAT.API.Features.JobApplications.Commands.CreateApplication;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JAT.IntegrationTests.Features.JobApplications.Commands;

public class CreateApplicationTests(
    CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task CreateApplication_WithValidData_ReturnsCreated()
    {
        var request = new CreateApplicationRequest(
            Title: "ExampleTitle",
            Notes: "ExampleNotes",
            Status: JobApplicationStatus.Applied,
            Url: "https://example.com/job",
            AppliedAt: DateTimeOffset.UtcNow,
            SalaryMin: 5000,
            SalaryMax: 7000);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/applications", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var location = response.Headers.Location;
        Assert.NotNull(location);

        var id = Guid.Parse(location.Segments.Last());

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var createdItem = await context.JobApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        Assert.NotNull(createdItem);
        Assert.Equal(request.Title, createdItem.Title);
    }

    [Theory]
    [InlineData("Senior .NET Developer", 500, 100)]
    [InlineData("Senior .NET Developer", -100, 100)]
    [InlineData("Senior .NET Developer", 500, -100)]
    [InlineData("", 500, 700)]
    public async Task CreateApplication_WithInvalidData_ReturnsBadRequest(
        string title,
        decimal salaryMin,
        decimal salaryMax)
    {
        var request = new CreateApplicationRequest(
            Title: title,
            Notes: "Remote role",
            Status: JobApplicationStatus.Applied,
            Url: "https://example.com/job",
            AppliedAt: DateTimeOffset.UtcNow,
            SalaryMin: salaryMin,
            SalaryMax: salaryMax);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/applications", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
