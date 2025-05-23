namespace Vjezba.Model
{
    public class Answer
    {
        public string Player { get; set; } = string.Empty;

        public string PlayersAnswer { get; set; } = string.Empty;

        public int TimeRemaining { get; set; } = 0;

        public int Score { get; set; } = 0;

        public override string ToString()
        {
            return $"Player: {Player}, Answer: {PlayersAnswer}, Time Remaining: {TimeRemaining}, Score: {Score}";
        }
    } 
}