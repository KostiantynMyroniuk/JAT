using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Features.JobApplications.Commands.UpdateApplication;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace JAT.IntegrationTests.Features.JobApplications.Commands
{
    public class UpdateApplicationTests(
        CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _httpClient = factory.CreateClient();

        [Fact]
        public async Task UpdateApplication_WithValidData_ReturnsNoContent()
        {
            var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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

            context.JobApplications.Add(application);
            await context.SaveChangesAsync();

            var request = new UpdateApplicationRequest(
                Title: "ExampleTitleUpdated",
                Notes: "ExampleNotesUpdated",
                Status: JobApplicationStatus.Ghosted,
                Url: "https://example.com/job",
                AppliedAt: DateTimeOffset.UtcNow,
                SalaryMin: 400,
                SalaryMax: 900);

            var result = await _httpClient.PutAsJsonAsync($"/api/v1/applications/{application.Id}", request);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

            var updatedApplication = await context.JobApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == application.Id);

            Assert.Equal(request.Title, updatedApplication.Title);
            Assert.Equal(request.Notes, updatedApplication.Notes);
            Assert.Equal(request.Status, updatedApplication.Status);
        }
    }
}
