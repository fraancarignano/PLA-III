namespace PLA_III.DataTransferObjects
{
    public class StartGameResponse
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public DateTime CreateAt { get; set; }

        public string Message { get; set; }
    }
}
