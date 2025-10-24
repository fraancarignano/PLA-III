// Data/GameDbContext.cs
using Microsoft.EntityFrameworkCore;
using PLA_III.Models;
using System.Linq;

namespace PLA_III.Data
{
    // 1. Debe heredar de DbContext
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Attempt> Attempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Player)          
                .WithMany(p => p.Games)
                
                .HasForeignKey(g => g.PlayerId)
                .IsRequired();

            
            modelBuilder.Entity<Attempt>()
                .HasOne(a => a.Game)            
                .WithMany(g => g.Attempts)
                
                .HasForeignKey(a => a.GameId)
                        .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}