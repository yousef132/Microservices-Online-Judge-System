using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Carter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Users.API.Services;
using static Users.API.Feature.User.RefreshToken;

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

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/sign-in", async (IUserService userService, CreateUserRequestDto requestDto) =>
            {
                // Validate request
                var errors = Validate(requestDto);
                if (errors.Any())
                    return Results.BadRequest(errors);

                // Call the service
                Guid signin = await userService.CreateUserAsync(requestDto);

                // Return response
                return Results.Ok(signin);
            })
              .WithTags("Auth")
              .Produces<Guid>(StatusCodes.Status200OK) // success response
              .Produces<IEnumerable<string>>(StatusCodes.Status400BadRequest); // validation errors
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
