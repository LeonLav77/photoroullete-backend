using System.ComponentModel.DataAnnotations;

namespace Vjezba.Model
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        public string Player { get; set; } = string.Empty;

        public string PlayersAnswer { get; set; } = string.Empty;

        public int TimeRemaining { get; set; } = 0;

        public int Score { get; set; } = 0;

        // Foreign key for Round
        public int RoundId { get; set; }
        public virtual Round Round { get; set; }

        public override string ToString()
        {
            return $"Player: {Player}, Answer: {PlayersAnswer}, Time Remaining: {TimeRemaining}, Score: {Score}";
        }
    }
}