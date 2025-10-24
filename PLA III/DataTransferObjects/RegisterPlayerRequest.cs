using System.ComponentModel.DataAnnotations;

namespace PLA_III.DataTransferObjects
{
    public class RegisterPlayerRequest
    {
        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es un campo requerido.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "La edad es un campo requerido.")]
        [Range(1, 150, ErrorMessage = "La edad debe ser un valor positivo.")]
        public int Age { get; set; }
    }
}
