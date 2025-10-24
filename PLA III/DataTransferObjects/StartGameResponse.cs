namespace PLA_III.DataTransferObjects
{
    public class StartGameResponse
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public DateTime CreateAt { get; set; }

        // Mensaje opcional para informar errores de negocio (ej: "Usuario no encontrado" o "Ya existe un juego activo")
        public string Message { get; set; }
    }
}
