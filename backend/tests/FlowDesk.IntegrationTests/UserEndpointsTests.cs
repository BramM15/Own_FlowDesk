using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;
using FlowDesk.API;
using FlowDesk.Infrastructure.Database;
using FlowDesk.API.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Entities;
using System.Data.Common;

namespace FlowDesk.IntegrationTests
{
    public class UserEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UserEndpointsTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // 1. Verwijder de standaard opties
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // 2. Verwijder de onderliggende database-connectie van PostgreSQL
                    var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
                    if (dbConnectionDescriptor != null) services.Remove(dbConnectionDescriptor);

                    // 3. DEZE ONTBRACK! Verwijder de verborgen .NET 8 configuratie van PostgreSQL
                    var configDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<ApplicationDbContext>));
                    if (configDescriptor != null) services.Remove(configDescriptor);

                    // 4. Voeg nu pas veilig InMemory toe
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));
                });
            });
        }

        [Fact]
        public async Task CreateAndListUser_Works()
        {
            var client = _factory.CreateClient();
            Guid validDepartmentId;

            // --- STAP 1: Zorg dat we een geldige afdeling hebben in de test-database ---
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated(); // Zorgt dat InMemory tabellen bestaan

                // Maak een test-afdeling aan (kijk even of jouw constructor 1 of 2 strings verwacht)
                var testDept = new Department("Test IT", "Test Beschrijving");
                db.Departments.Add(testDept);
                await db.SaveChangesAsync();

                // Bewaar dit ID, want deze bestaat nu ECHT in de database!
                validDepartmentId = testDept.Id; 
            }

            // --- STAP 2: Voer het verzoek uit met het GELDIGE ID ---
            var request = new CreateUserRequest(
                "Alice", 
                "Smith", 
                "alice@example.com", 
                "hash", 
                UserRole.User, 
                validDepartmentId); // <-- Hier gebruiken we het echte ID!

            var response = await client.PostAsJsonAsync("/users", request);
    
            // Optioneel: als het faalt, kun je hiermee de precieze foutmelding in je test zien:
            var errorText = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            // --- STAP 3: Controleer of de gebruiker is opgeslagen ---
            var users = await client.GetFromJsonAsync<List<User>>("/users");
            Assert.Contains(users, u => u.Email == "alice@example.com");
        }
    }
}
