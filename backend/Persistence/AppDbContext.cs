using backend.Application.Auth.Models;
using backend.Domain;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<UserRole>()
              .HasOne(ur => ur.User)
              .WithMany()
              .HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>()
              .HasOne(ur => ur.Role)
              .WithMany(r => r.UserRoles)
              .HasForeignKey(ur => ur.RoleId);
        }
    }
}