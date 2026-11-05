using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace JAT.IntegrationTests.Features.JobApplications.Commands
{
    public class DeleteApplicationTests(
        CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _httpClient = factory.CreateClient();

        [Fact]
        public async Task DeleteApplication_WithValidData_ReturnsNoContent()
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

            var result = await _httpClient.DeleteAsync($"/api/v1/applications/{application.Id}");
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

            var deletedApplication = await context.JobApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == application.Id);

            Assert.Null(deletedApplication);
        }
    }
}
