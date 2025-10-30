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


            string connectionString = "Data Source=DESKTOP-VVVV704\\SERVIDOR3;Initial Catalog=JuegoPLA2;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;";

            optionsBuilder.UseSqlServer(connectionString); 

            return new GameDbContext(optionsBuilder.Options);
        }
    }
}