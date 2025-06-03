using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LearnQuestV1.Core.Models;

namespace LearnQuestV1.EF.Application
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<UserVisitHistory> UserVisitHistory { get; set; } = null!;
        public DbSet<BlacklistToken> BlacklistTokens { get; set; } = null!;
        public DbSet<AccountVerification> AccountVerifications { get; set; } = null!;
        public DbSet<UserDetail> UserDetails { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1) Unique index on EmailAddress
            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmailAddress)
                .IsUnique();


        }
    }
}
