using System.Reflection;

namespace Users.API.Models;

public class User
{
    public Guid Id { get; set; }
    public Guid Keycloak_Id { get; set; } 
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string DisplayName { get; set; } = null!;
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; } 
    public string? GithubUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public bool IsPublicProfile { get; set; } = true;
    public string? ProfilePictureUrl { get; set; }

    public UserStatus RankName { get; set; } = UserStatus.UnRanked; // ex: pupil
    public short Rating { get; set; } = 0; // ex: 1200 
    public Gender? Gender { get; set; }

}
