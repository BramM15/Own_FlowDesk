using System;
using System.Threading.Tasks;
using FlowDesk.Infrastructure.Database;
using FlowDesk.Infrastructure.Repositories;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlowDesk.IntegrationTests;

public class DepartmentRepositoryTests
{
    [Fact]
    public async Task Add_Get_Update_Delete_Workflow()
    {
        // Read connection string from environment. If not provided, skip the test.
        var connString = Environment.GetEnvironmentVariable("INTEGRATION_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connString))
        {
            return;
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connString)
            .Options;

        // create schema
        using (var context = new ApplicationDbContext(options))
        {
            context.Database.EnsureCreated();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var repo = new DepartmentRepository(context);

            var dept = new Department("Integration", "Integration test dept");
            await repo.AddAsync(dept);

            var fetched = await repo.GetByIdAsync(dept.Id);
            Assert.NotNull(fetched);
            Assert.Equal("Integration", fetched.Name);

            var all = await repo.GetAllAsync();
            Assert.Contains(all, d => d.Id == dept.Id);

            // update
            dept.Update("IntegrationUpdated", "Updated desc");
            await repo.UpdateAsync(dept);

            var updated = await repo.GetByIdAsync(dept.Id);
            Assert.Equal("IntegrationUpdated", updated.Name);

            // delete
            await repo.DeleteAsync(dept.Id);

            var afterDelete = await repo.GetByIdAsync(dept.Id);
            Assert.Null(afterDelete);
        }
    }
}