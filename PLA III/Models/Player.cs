namespace PLA_III.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        // CORRECCIÓN: Se elimina la línea 'public int Game { get; set; }'

        // Propiedad de Navegación: Colección de juegos asociados a este jugador
        public ICollection<Game> Games { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}