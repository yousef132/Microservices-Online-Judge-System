using CoreJudge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreJudge.Infrastructure.Configurations
{
    internal class ContestConfiguration : IEntityTypeConfiguration<Contest>
    {
        public void Configure(EntityTypeBuilder<Contest> builder)
        {
            // user table already in user.service.db 
            //builder.HasOne(c => c.ContestSetterId)
            //    .WithMany(u => u.Contests)
            //    .HasForeignKey(c => c.ProblemSetterId)
            //    .OnDelete(DeleteBehavior.Restrict);

            builder.Property(c => c.Name)
                .IsRequired();
        }
    }



}
