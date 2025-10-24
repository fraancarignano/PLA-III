namespace PLA_III.Models
{
    public class Attempt
    {
        public int AttemptId { get; set; }

        // CORRECCIÓN 1: Clave Foránea (FK). Nombre estándar para EF Core.
        public int GameId { get; set; }

        // CORRECCIÓN 2: Propiedad de Navegación (Objeto). Debe ser singular.
        public Game Game { get; set; }

        public int AttemptedNumber { get; set; }
        public int Picas { get; set; }
        public int Famas { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}