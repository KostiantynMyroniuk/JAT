using System.Net;
using System.Net.Http.Json;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Features.JobApplications.Commands.SetApplicationTags;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JAT.IntegrationTests.Features.JobApplications.Commands;

public class SetApplicationTagsTests(
    CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task SetApplicationTags_WithValidData_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var oldTag = Tag.Create($"Old-{suffix}");
        var keptTag = Tag.Create($"Kept-{suffix}");
        var newTag = Tag.Create($"New-{suffix}");

        var application = new JobApplication()
        {
            Title = "ExampleTitle",
            Notes = "ExampleNotes",
            Status = JobApplicationStatus.Applied,
            Url = "https://example.com/job",
            AppliedAt = DateTimeOffset.UtcNow,
            SalaryMin = 5000,
            SalaryMax = 7000
        };

        application.SetTags([oldTag, keptTag]);

        await using (var arrangeScope = factory.Services.CreateAsyncScope())
        {
            var arrangeContext = arrangeScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            arrangeContext.Tags.AddRange(oldTag, keptTag, newTag);
            arrangeContext.JobApplications.Add(application);
            await arrangeContext.SaveChangesAsync();
        }

        var request = new SetApplicationTagsRequest([keptTag.Id, newTag.Id]);

        var result = await _httpClient.PutAsJsonAsync(
            $"/api/v1/applications/{application.Id}/tags",
            request);

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var applicationWithTags = await context.JobApplications
            .AsNoTracking()
            .Include(a => a.Tags)
            .SingleAsync(a => a.Id == application.Id);

        Assert.Equal(
            new[] { keptTag.Id, newTag.Id }.OrderBy(x => x),
            applicationWithTags.Tags.Select(x => x.Id).OrderBy(x => x));
    }

    [Fact]
    public async Task SetApplicationTags_WhenApplicationNotFound_ReturnsNotFound()
    {
        var request = new SetApplicationTagsRequest([Guid.NewGuid()]);

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/v1/applications/{Guid.NewGuid()}/tags",
            request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
