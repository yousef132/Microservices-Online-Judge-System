using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.API.Models;

namespace Users.API.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u=> u.Id);
        builder.HasIndex(u=> u.Keycloak_Id).IsUnique();
        builder.HasIndex(u=> u.Username).IsUnique();
        builder.HasIndex(u=> u.Email).IsUnique();
        
    }
}