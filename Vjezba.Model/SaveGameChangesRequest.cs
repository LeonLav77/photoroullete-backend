namespace Vjezba.Model
{
    public class SaveGameChangesRequest
    {
        public int GameId { get; set; }
        public Dictionary<string, int> PlayerScores { get; set; } = new();
        public Dictionary<int, int> AnswerScores { get; set; } = new();
        public List<int> DeletedRounds { get; set; } = new();
        public int? Excitement { get; set; }
    }
}