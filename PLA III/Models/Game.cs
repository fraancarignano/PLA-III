namespace PLA_III.Models
{
    public class Game
    {
        public int GameId { get; set; }

        // CORRECCIÓN 1: La FK debe llamarse PlayerId
        public int PlayerId { get; set; }

        // CORRECCIÓN 2: La propiedad de navegación debe ser el objeto Player (en singular)
        // Y su nombre debe ser "Player" para que HasOne(g => g.Player) funcione.
        public Player Player { get; set; }

        // (Opcional) Eliminar esta línea, ya que no representa un dato a persistir
        // public int Attempt { get; set; } 

        public int SecretNumber { get; set; }
        public string CreateAt { get; set; }

        public bool IsFinished { get; set; }

        // Colección de intentos (Navegación del lado 'uno' de la relación 1:N)
        public ICollection<Attempt> Attempts { get; set; }
    }
}