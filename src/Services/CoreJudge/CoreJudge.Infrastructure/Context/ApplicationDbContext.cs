using CoreJudge.Domain.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CoreJudge.Infrastructure.Context
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Contest> Contests { get; set; }
        public DbSet<UserContest> UserContestRegistrations { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<ProblemImage> ProblemImages { get; set; }
        public DbSet<Testcase> Testcases { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<ProblemTopic> ProblemTopics { get; set; }
        public DbSet<ProblemLangeuageTemplates> ProblemLangeuageTemplates { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // generate outbox and inbox db tables
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
