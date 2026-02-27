using System.ComponentModel.DataAnnotations;
using Carter;
using Users.API.Services;

namespace Users.API.Feature.User;

public static class UserRolesModule
{
    // ========================
    // DTOs
    // ========================
    private record ManageUserRoleRequest(
        [Required] string UserId,
        [Required] string RoleName
    );

    private record ManageUserRoleResponse(
        string UserId,
        string RoleName,
        string Action
    );

    public class UserRolesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // ========================
            // Assign role
            // ========================
            app.MapPost("/users/assign-role", async (IUserService userService, ManageUserRoleRequest request) =>
            {
                var errors = Validate(request);
                if (errors.Any())
                    return Results.BadRequest(errors);

                await userService.AddRoleToUserAsync(Guid.Parse(request.UserId), request.RoleName);

                return Results.Ok(new ManageUserRoleResponse(request.UserId, request.RoleName, "Assigned"));
            })
            .WithTags("UserRoles")
            .Produces<ManageUserRoleResponse>(StatusCodes.Status200OK)
            .RequireAuthorization("Admin")
            .Produces<IEnumerable<string>>(StatusCodes.Status400BadRequest);

            // ========================
            // Remove role
            // ========================
            app.MapPost("/users/remove-role", async (IUserService userService, ManageUserRoleRequest request) =>
            {
                var errors = Validate(request);
                if (errors.Any())
                    return Results.BadRequest(errors);

                await userService.RemoveRoleFromUserAsync(Guid.Parse(request.UserId), request.RoleName);

                return Results.Ok(new ManageUserRoleResponse(request.UserId, request.RoleName, "Removed"));
            })
            .WithTags("UserRoles")
            .RequireAuthorization("Admin")
            .Produces<ManageUserRoleResponse>(StatusCodes.Status200OK)
            .Produces<IEnumerable<string>>(StatusCodes.Status400BadRequest);
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