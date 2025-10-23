using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.SqlServer;
using PLA_III.Models;


namespace PLA_III.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Attempt> Attempts { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Player>()
                .HasKey(p => p.PlayerId); 

            
            modelBuilder.Entity<Game>()
                .HasKey(g => g.GameId); 

            
            modelBuilder.Entity<Attempt>()
                .HasKey(a => a.AttemptId); 

            

            
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Player)          
                .WithMany(p => p.Games)         
                .HasForeignKey(g => g.Player) 
                .IsRequired();                  

            
            modelBuilder.Entity<Attempt>()
                .HasOne(a => a.Game)            
                .WithMany(g => g.Attempts)      
                .HasForeignKey(a => a.Game)   
                .IsRequired();                  

           
            base.OnModelCreating(modelBuilder);
        }


    }
}
