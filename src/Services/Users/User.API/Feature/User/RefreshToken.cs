using System.ComponentModel.DataAnnotations;
using Carter;
using Users.API.Services;

namespace Users.API.Feature.User;

public class RefreshToken
{
    public record RefreshTokenRequestDto
    {
        public string Token { get; set; } = string.Empty;
    }
    public class RefreshTokenResponse(
        string AccessToken, 
        string RefreshToken
    );
    public class RefreshTokenEndpoint : ICarterModule
    {


        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/refresh-token", async (IUserService userService, RefreshTokenRequestDto  requestDto,CancellationToken cancellationToken) =>
            {
                // Validate request
                var errors = Validate(requestDto);
                if (errors.Any())
                    return Results.BadRequest(errors);

                // Call the service
                var refreshTokenResponse = await userService.RefreshUserAsnc(requestDto,cancellationToken);

                // Return response
                return Results.Ok(refreshTokenResponse);
            })
              .WithTags("Auth")         
             .Produces<RefreshTokenResponse>(StatusCodes.Status200OK) // success response
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