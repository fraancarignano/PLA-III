namespace PLA_III.Models
{
    public class Game
    {
        public int GameId { get; set; }

        public int PlayerId { get; set; }

        public Player Player { get; set; }

        public string SecretNumber { get; set; }
        public DateTime CreateAt { get; set; }

        public bool IsFinished { get; set; }

        public ICollection<Attempt> Attempts { get; set; }
    }
}