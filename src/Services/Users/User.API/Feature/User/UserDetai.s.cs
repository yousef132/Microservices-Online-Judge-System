using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Carter;
using Microsoft.AspNetCore.Mvc;
using Users.API.Services;
using static Users.API.Feature.User.RefreshToken;

namespace Users.API.Feature.User;

public class UserDetails
{
    public sealed record UserDetailsDto(
        Guid Id,
        string Username,
        string Email,
        DateTime LastLogin,
        DateTime CreatedAt,
        string DisplayName,
        string? Bio,
        string? LinkedInUrl,
        string? GithubUrl,
        string? FacebookUrl,
        bool IsPublicProfile,
        string? ProfilePictureUrl
    );
    public class UserDetailsEndpoint : ICarterModule
    {


        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/auth/details/{userId}", async (IUserService userService,[FromRoute] string userId, CancellationToken cancellationToken) =>
            {
                // // Validate request
                // var errors = Validate(requestDto);
                // if (errors.Any())
                //     return Results.BadRequest(errors);
                
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Call the service
               var details = await userService.GetUserAsync(userId,cancellationToken);

                // Return response
                return Results.Ok(details);
            })
              .WithTags("Auth")
             .Produces<UserDetailsDto>(StatusCodes.Status200OK) // success response
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