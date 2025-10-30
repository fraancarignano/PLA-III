namespace PLA_III.DataTransferObjects
{
    public class GuessNumberResponse
    {
        public int Picas { get; set; }
        public int Famas { get; set; }

        public string Message { get; set; }

        public int GameId { get; set; }
        public string AttemptedNumber { get; set; }
    }
}
