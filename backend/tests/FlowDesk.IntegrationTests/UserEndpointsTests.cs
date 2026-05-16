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
        
        [Fact]
        public async Task GetUsersByDepartment_Works()
        {
            var client = _factory.CreateClient();
            Guid validDepartmentId;
            Guid testUserId;

            // 1. Seed data
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                var testDept = new Department("HR", "Human Resources");
                db.Departments.Add(testDept);
                
                var testUser = new User("Bob", "Builder", "bob@hr.com", "hash", UserRole.User, testDept.Id);
                db.Users.Add(testUser);
                await db.SaveChangesAsync();

                validDepartmentId = testDept.Id;
                testUserId = testUser.Id;
            }

            // 2. Voer het GET request uit
            var users = await client.GetFromJsonAsync<List<User>>($"/users/department/{validDepartmentId}");
            
            // 3. Controleer de resultaten via Email (omdat ID een private setter heeft)
            Assert.NotNull(users);
            Assert.Contains(users, u => u.Email == "bob@hr.com");
        }
        
        [Fact]
        public async Task UpdateUser_Works()
        {
            var client = _factory.CreateClient();
            Guid userId;
            Guid originalDeptId;
            Guid newDeptId;

            // 1. Seed data: We maken 2 afdelingen en 1 gebruiker
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                var dept1 = new Department("Oude Afdeling", "Oud");
                var dept2 = new Department("Nieuwe Afdeling", "Nieuw");
                db.Departments.AddRange(dept1, dept2);
                
                var user = new User("Charlie", "Chaplin", "charlie@test.com", "hash", UserRole.User, dept1.Id);
                db.Users.Add(user);
                await db.SaveChangesAsync();

                userId = user.Id;
                originalDeptId = dept1.Id;
                newDeptId = dept2.Id;
            }

            // 2. Maak de update request body (we veranderen de Rol en de Afdeling)
            var updateRequest = new 
            { 
                FirstName = "Charlie", // Stuur mee als jouw DTO deze verwacht
                LastName = "Chaplin",
                Email = "charlie@test.com",
                Role = UserRole.Admin, // We maken hem Admin
                DepartmentId = newDeptId // We verplaatsen hem naar afdeling 2
            };

            // 3. Voer het PUT request uit
            var response = await client.PutAsJsonAsync($"/users/{userId}", updateRequest);
            response.EnsureSuccessStatusCode();

            // 4. Controleer of de database echt is aangepast
            var updatedUser = await client.GetFromJsonAsync<User>($"/users/{userId}");
            Assert.NotNull(updatedUser);
            Assert.Equal(UserRole.Admin, updatedUser.Role);
            Assert.Equal(newDeptId, updatedUser.DepartmentId);
        }
        
        [Fact]
        public async Task DeleteUser_Works()
        {
            var client = _factory.CreateClient();
            Guid userId;

            // 1. Seed data
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                var testDept = new Department("Finance", "Finance Dept");
                db.Departments.Add(testDept);
                
                var user = new User("Diana", "Prince", "diana@finance.com", "hash", UserRole.User, testDept.Id);
                db.Users.Add(user);
                await db.SaveChangesAsync();

                userId = user.Id;
            }

            // 2. Voer het DELETE request uit
            var deleteResponse = await client.DeleteAsync($"/users/{userId}");
            
            // Controleer of de API 'No Content' (204) teruggeeft, wat de standaard is voor een succesvolle delete
            Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // 3. Probeer de gebruiker nogmaals op te halen
            var getResponse = await client.GetAsync($"/users/{userId}");
            
            // Dit zou nu een 404 Not Found moeten zijn!
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
