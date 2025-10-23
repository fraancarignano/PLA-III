namespace PLA_III.Models
{
    public class Game
    {
        public int GameId { get; set; }
        public ICollection<Attempt> Attempts { get; set; }
        public Player Player { get; set; }
        public int SecretNumber { get; set; }
        public string CreateAt { get; set; }

        public bool IsFinished { get; set; }
    }
}
