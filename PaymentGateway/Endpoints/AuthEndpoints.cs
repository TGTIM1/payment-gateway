using Microsoft.AspNetCore.Mvc;
using PaymentGateway.DTOs;
using PaymentGateway.Services;

namespace PaymentGateway.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/telegram", (
            [FromBody] TelegramAuthRequestDto request,
            ITelegramAuthService authService) =>
        {
            bool isValid = authService.ValidateInitData(request.InitData);

            if (!isValid)
            {
                return Results.Unauthorized();
            }

            var user = authService.ParseUserData(request.InitData);
            return Results.Ok(new { message = "Authenticated successfully", user });
        });
    }
}