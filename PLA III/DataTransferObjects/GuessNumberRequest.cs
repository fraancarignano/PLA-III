using System.ComponentModel.DataAnnotations;

namespace PLA_III.DataTransferObjects
{
    public class GuessNumberRequest
    {
        [Required(ErrorMessage = "El ID del juego es requerido para registrar un intento.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del juego debe ser un valor válido.")]
        public int GameId { get; set; }

        [Required(ErrorMessage = "Se requiere el número del intento (4 dígitos).")]
        
        public string AttemptedNumber { get; set; }
    }
}
