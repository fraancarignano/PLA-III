using System.ComponentModel.DataAnnotations;

namespace PLA_III.DataTransferObjects
{
    public class StartGameRequest
    {
        [Required(ErrorMessage = "El ID del jugador es requerido para iniciar una partida.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del jugador debe ser un valor válido.")]
        public int PlayerId { get; set; }
    }
}
