using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


using PLA_III.Data;

namespace PLA_III
{
    
    public class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
    {
        public GameDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();

            
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=PicasFamasDB;Trusted_Connection=True;";
            
            optionsBuilder.UseSqlServer(connectionString); 

            return new GameDbContext(optionsBuilder.Options);
        }
    }
}