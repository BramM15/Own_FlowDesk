// backend/src/FlowDesk.Application/Services/AuthHandler.cs

using FlowDesk.Application.Interfaces;

namespace FlowDesk.Application.Services;

public class AuthHandler
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly UserHandler _userHandler; // Of UserHandler

    public AuthHandler(
        UserHandler userHandler,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userHandler = userHandler;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        // 1. Haal gebruiker op
        var user = await _userHandler.GetByEmailAsync(email);
        if (user is null) throw new Exception("Invalid credentials");

        // 2. Verifieer wachtwoord
        var isPasswordValid = _passwordHasher.Verify(password, user.PasswordHash);
        if (!isPasswordValid) throw new Exception("Invalid credentials");

        // 3. Genereer JWT (hierin zit de Role verwerkt dankzij je JwtProvider implementatie)
        var token = _jwtProvider.Generate(user);

        return token;
    }
}