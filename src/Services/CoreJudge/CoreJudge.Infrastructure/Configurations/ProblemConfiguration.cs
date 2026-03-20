using CoreJudge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreJudge.Infrastructure.Configurations
{
    internal class ProblemConfiguration : IEntityTypeConfiguration<Problem>
    {
        public void Configure(EntityTypeBuilder<Problem> builder)
        {
            //builder.HasOne(p => p.ProblemSetter)
            //       .WithMany(u => u.Problems)
            //       .HasForeignKey(p => p.ProblemSetterId)
            //       .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Contest)
                   .WithMany(c => c.Problems)
                   .HasForeignKey(p => p.ContestId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Images)
                .WithOne(pi => pi.Problem)
                .HasForeignKey(pi => pi.ProblemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(p => p.Name)
            .IsRequired();
        }
    }
}
