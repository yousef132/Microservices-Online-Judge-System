using CoreJudge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreJudge.Infrastructure.Configurations
{
    public class SubmitConfiguration : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            // user table already in user.service.db

            //builder.HasOne(s => s.User)
            //    .WithMany(u => u.Submissions)
            //    .HasForeignKey(s => s.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Problem)
                .WithMany(p => p.Submissions)
                .HasForeignKey(s => s.ProblemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Contest)
                .WithMany(c => c.Submissions)
                .HasForeignKey(s => s.ContestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }



}
