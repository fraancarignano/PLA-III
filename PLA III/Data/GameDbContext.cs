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

        // Constructor, DbSets, etc.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ... (Configuración de PKs) ...

            // --- Relaciones Uno a Muchos (1:N) ---

            // 1. Relación Player (1) a Game (N): Un juego pertenece a un jugador.
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Player)          // Correcto: Referencia al OBJETO de navegación (Player)
                .WithMany(p => p.Games)
                // CORRECCIÓN: Referencia a la PROPIEDAD NUMÉRICA (int) de la FK
                .HasForeignKey(g => g.PlayerId)
                .IsRequired();

            // 2. Relación Game (1) a Attempt (N): Un intento pertenece a un juego.
            modelBuilder.Entity<Attempt>()
                .HasOne(a => a.Game)            // Correcto: Referencia al OBJETO de navegación (Game)
                .WithMany(g => g.Attempts)
                // CORRECCIÓN: Referencia a la PROPIEDAD NUMÉRICA (int) de la FK
                .HasForeignKey(a => a.GameId)
                        .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}