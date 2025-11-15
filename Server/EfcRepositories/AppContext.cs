using Entities;
using Microsoft.EntityFrameworkCore;

namespace EfcRepositories;

public class AppContext : DbContext
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1234"
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("forum");
    }
}