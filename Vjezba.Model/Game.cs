using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vjezba.Model
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        public string Code { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public List<Round> Rounds { get; set; } = new List<Round>();

        [NotMapped]
        public Dictionary<string, string> Images { get; set; } = new Dictionary<string, string>();

        public int CurrentRound { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? FinishedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Player> PlayersCollection { get; set; } = new List<Player>();
        public virtual ICollection<Round> RoundsCollection { get; set; } = new List<Round>();
    }
}