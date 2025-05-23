using System.ComponentModel.DataAnnotations;

namespace Vjezba.Model
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        public string Code { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public List<Round> Rounds { get; set; } = new List<Round>();

        public Dictionary<string, string> Images { get; set; } = new Dictionary<string, string>();

        public int CurrentRound { get; set; } = 0;
    } 
}