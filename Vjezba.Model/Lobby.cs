using System.ComponentModel.DataAnnotations;

namespace Vjezba.Model
{
    public class Lobby
    {
        [Key]
        public int Id { get; set; }

        public string Code { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool AllPlayersReady()
        {
            return Players.Count > 0 && Players.All(player => player.IsReady);
        }
    } 
}