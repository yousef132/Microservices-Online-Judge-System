using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.API.Models;

namespace Users.API.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table & key
        builder.HasKey(u => u.Id);

        // Indexes (unique)
        builder.HasIndex(u => u.Keycloak_Id).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        // Basic properties
        builder.Property(u => u.Keycloak_Id)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(254);
        
        // Profile
        builder.Property(u => u.ProfilePictureUrl)
            .HasMaxLength(2048)
            .IsRequired(false);

        builder.Property(u => u.Bio)
            .HasMaxLength(1000)
            .IsRequired(false);
    }
}