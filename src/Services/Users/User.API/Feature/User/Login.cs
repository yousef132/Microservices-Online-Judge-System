    using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
    using Carter;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Users.API.Services;
    using static Users.API.Feature.User.RefreshToken;

    namespace Users.API.Feature.User;

    public static class Login
    {
        // ========================
        // DTOs
        // ========================


        public record LoginUserResponse(
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
                    Console.WriteLine(JsonSerializer.Serialize(loginResponse));
                    // Return response
                    return Results.Ok(loginResponse);
                })
                  .WithTags("Auth")
                  .Produces<LoginUserResponse>(StatusCodes.Status200OK) // success response
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
public class TestModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/test-working", () => "It works!")
            .WithName("TestWorking");
    }
}
