using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Carter;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Users.API.Services;

namespace Users.API.Feature.User;

public static class Login
{
    // ========================
    // DTOs
    // ========================


    public class LoginUserResponse(
         string AccessToken, 
         string RefreshToken
    );

    public record LoginUserRequestDto(
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address")]
        string Email,

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        string Password
    );

    // ========================
    // Carter Module using ICarterModule
    // ========================
    public class LoginEndpoint : ICarterModule
    {
        private readonly IUserService _userService;

        public LoginEndpoint(IUserService userService)
        {
            _userService = userService;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/login", async (IUserService userService, LoginUserRequestDto requestDto) =>
            {
                // Validate request
                var errors = Validate(requestDto);
                if (errors.Any())
                    return Results.BadRequest(errors);

                // Call the service
                var loginResponse = await userService.LoginUserAsync(requestDto);

                // Return response
                return Results.Ok(loginResponse);
            });
        }

        // -------------------------------
        // Validation helper
        // -------------------------------
        private static IEnumerable<string> Validate<T>(T dto)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(dto, null, null);
            Validator.TryValidateObject(dto, context, validationResults, true);
            return validationResults.Select(vr => vr.ErrorMessage ?? string.Empty);
        }
    }
}
