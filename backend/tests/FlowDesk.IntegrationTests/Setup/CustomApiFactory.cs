using FlowDesk.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace FlowDesk.IntegrationTests.Setup;

// Deze factory start onze API op in het geheugen + koppelt het aan de Testcontainer
public class CustomApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("flowdesk_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Zorg ervoor dat de database gemigreerd is voor de testen
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Injecteer de missende JWT-instelling specifiek voor de testomgeving
        builder.UseSetting("JwtSettings:Secret", "dit_is_een_super_geheim_test_secret_van_minimaal_32_tekens!");

        builder.ConfigureServices(services =>
        {
            // Verwijder de bestaande DbContext configuratie
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            // Voeg de DbContext toe met de connectiestring van de Testcontainer
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }
}