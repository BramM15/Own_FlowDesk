// backend/src/FlowDesk.API/Middleware/GlobalExceptionHandler.cs
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Er is een fout opgetreden: {Message}", exception.Message);
        
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
            Detail = exception.Message
        };
        
        if (exception is ArgumentException || exception.Message.Contains("Invalid") || exception.Message.Contains("exists"))
        {
            // Bijvoorbeeld: "Email already exists" of "Invalid credentials" uit je eerdere code
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Ongeldige aanvraag (Bad Request)";
        }
        else if (exception is UnauthorizedAccessException)
        {
            problemDetails.Status = StatusCodes.Status401Unauthorized;
            problemDetails.Title = "Niet geautoriseerd";
        }
        else
        {
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "Interne Server Fout";
        }
        
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true; 
    }
}