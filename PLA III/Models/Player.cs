namespace PLA_III.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        public ICollection<Game> Games { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}