namespace PLA_III.Models
{
    public class Attempt
    {
        public int AttemptId { get; set; }
        public Game Game { get; set; }
        public int AttemptedNumber { get; set; }
        public int Picas {  get; set; }
        public int Famas { get; set; }
        public DateTime AttemptDate { get; set; }

    }
}
