using System.IdentityModel.Tokens.Jwt;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;

namespace FlowDesk.UnitTests.Authenticatie;

public class JwtProviderTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtProvider _jwtProvider;

    public JwtProviderTests()
    {
        // Maak de configuratie-instellingen aan die JwtProvider.cs verwacht te lezen
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "JwtSettings:Secret", "DitIsEenSuperLangeEnGeheimeSleutelVoorDeJwtToken123!" },

            // Voeg hier eventuele overige JWT-velden toe als jouw code daar ook naar zoekt:
            { "JwtSettings:Issuer", "FlowDeskAuthServer" },
            { "JwtSettings:Audience", "FlowDeskApi" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _jwtProvider = new JwtProvider(_configuration);
    }

    [Fact]
    public void Generate_ShouldReturnValidJwtToken_WithCorrectClaims()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var user = new User(
            "John",
            "Doe",
            "john.doe@flowdesk.nl",
            "hashed_password_xyz",
            UserRole.Support,
            departmentId
        );

        // Act
        var token = _jwtProvider.Generate(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Controleer of het een geldige JWT tokenstructuur is
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.True(tokenHandler.CanReadToken(token));

        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Controleer de claims die JwtProvider erin heeft gestopt (sub, email, role)
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

        Assert.Equal(user.Id.ToString(), subClaim);
        Assert.Equal(user.Email, emailClaim);

        // Controleer of de configuratiewaarden juist zijn toegepast
        Assert.Equal("FlowDeskAuthServer", jwtToken.Issuer);
    }
}