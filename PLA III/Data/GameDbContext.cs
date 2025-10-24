// Data/GameDbContext.cs
using Microsoft.EntityFrameworkCore;
using PLA_III.Models;
using System.Linq; // Necesario si se usa en OnModelCreating, aunque no aquí.

namespace PLA_III.Data
{
    // AHORA HEREDA DE DbContext (¡La clave para que EF Core funcione!)
    public class GameDbContext : DbContext
    {
        // Constructor requerido para la inyección de dependencias en Program.cs
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        // --- Propiedades DbSet para las Tablas ---
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Attempt> Attempts { get; set; }

        // --- Configuración de Relaciones y Modelos ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Claves Primarias (PK) — Generalmente se infieren por convención (PlayerId, GameId, etc.)

            // 1. Relación Player (1) a Game (N)
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Player)
                .WithMany(p => p.Games)
                // CORRECCIÓN: Debe apuntar a la propiedad PlayerId (int), no a g.Player (objeto)
                .HasForeignKey(g => g.Player)
                .IsRequired();

            // 2. Relación Game (1) a Attempt (N)
            modelBuilder.Entity<Attempt>()
                .HasOne(a => a.Game)
                .WithMany(g => g.Attempts)
                // CORRECCIÓN: Debe apuntar a la propiedad GameId (int), no a a.Game (objeto)
                .HasForeignKey(a => a.Game)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}