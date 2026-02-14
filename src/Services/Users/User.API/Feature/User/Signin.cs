using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Carter;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Users.API.Services;

namespace Users.API.Feature.User;

public static class Signin
{
    // ========================
    // DTOs
    // ========================


    public sealed record CreateUserRequestDto(string Email,string DisplayName, string Password);

    // ========================
    // Carter Module using ICarterModule
    // ========================
    public class SigninEndpoint : ICarterModule
    {
        private readonly IUserService _userService;

        public SigninEndpoint(IUserService userService)
        {
            _userService = userService;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth", async (IUserService userService, CreateUserRequestDto requestDto) =>
            {
                // Validate request
                var errors = Validate(requestDto);
                if (errors.Any())
                    return Results.BadRequest(errors);

                // Call the service
                Guid signin = await userService.CreateUserAsync(requestDto);

                // Return response
                return Results.Ok(signin);
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
