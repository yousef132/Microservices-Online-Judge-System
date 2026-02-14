using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Users.API.Services;
using static Users.API.Feature.User.RefreshToken;

namespace Users.API.Feature.User;

public static class UpdateUser
{
    public sealed record UpdateUserDto(
        string? DisplayName,
        string? Bio,
        string? LinkedInUrl,
        string? GithubUrl,
        string? FacebookUrl,
        bool? IsPublicProfile,
        string? ProfilePictureUrl
    );
    
    public class UpdateUserEndpoint : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/auth", async (IUserService userService, UpdateUser.UpdateUserDto  requestDto,HttpContext ctx) =>
            {
                // Validate request
                var errors = Validate(requestDto);
                if (errors.Any())
                    return Results.BadRequest(errors);
                
                var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Call the service
                 await userService.UpdateUserAsync(requestDto, Guid.Parse(userId), CancellationToken.None);

                // Return response
                return Results.NoContent();
            })
              .WithTags("Auth")
             .Produces<NoContent>(StatusCodes.Status200OK) // success response
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