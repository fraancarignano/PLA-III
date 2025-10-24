namespace PLA_III.DataTransferObjects
{
    public class GuessNumberResponse
    {
        // Pistas numéricas generadas por la librería ESCMB.GuessCore
        public int Picas { get; set; }
        public int Famas { get; set; }

        // Mensaje detallado. Contendrá la pista (ej: "1 Pica, 2 Famas"), 
        // el mensaje de éxito ("¡Felicidades!") o el mensaje de error.
        public string Message { get; set; }

        // Se incluyen los IDs de referencia en la respuesta, como buena práctica
        public int GameId { get; set; }
        public string AttemptedNumber { get; set; }
    }
}
