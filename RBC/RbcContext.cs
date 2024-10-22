using Microsoft.EntityFrameworkCore;
using RBC.Models;

namespace RBC;

public class RbcContext : DbContext
{ 
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Genre> Genres { get; set; }

    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("MoviesDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>()
            .HasKey(m => m.MovieId);

        modelBuilder.Entity<Rating>()
            .HasKey(r => r.MovieId);

        modelBuilder.Entity<Tag>()
            .HasKey(t => t.TagId);
        
        modelBuilder.Entity<Genre>()
            .HasKey(t => t.GenreId);

        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Ratings)
            .WithOne()
            .HasForeignKey(r => r.MovieId);

        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Genres)
            .WithOne()
            .HasForeignKey(g => g.MovieId);

        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Tags)
            .WithOne()
            .HasForeignKey(t => t.MovieId);
    }
}