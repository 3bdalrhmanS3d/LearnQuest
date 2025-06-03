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
            modelBuilder.Entity<User>()
                .HasMany(u => u.AccountVerifications)
                .WithOne(av => av.User)
                .HasForeignKey(av => av.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.VisitHistories)
                .WithOne(vh => vh.User)
                .HasForeignKey(vh => vh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDetail)
                .WithOne(ud => ud.User)
                .HasForeignKey<UserDetail>(ud => ud.UserId);

            // تعريف الجداول وغيرها إن لزم
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<AccountVerification>().ToTable("AccountVerifications");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
            modelBuilder.Entity<UserVisitHistory>().ToTable("UserVisitHistories");
            modelBuilder.Entity<BlacklistToken>().ToTable("BlacklistTokens");
            modelBuilder.Entity<UserDetail>().ToTable("UserDetails");


        }
    }
}
