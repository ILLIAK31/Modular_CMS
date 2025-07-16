using backend.Application.Auth.Models;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<User> Users { get; set; } = null!;
    }
}