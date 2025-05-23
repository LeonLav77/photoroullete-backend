using System.ComponentModel.DataAnnotations;

namespace Vjezba.Model
{
    public class Player
    {
        [Key]
        public int Id { get; set; }

        public string ConnectionId { get; set; }

        public string Name { get; set; }

        public List<string> Images { get; set; } = new List<string>();

        public bool IsReady { get; set; } = false;

        public Player(string connectionId, string name)
        {
            ConnectionId = connectionId;
            Name = name;
        }

        override public string ToString()
        {
            return $"Id: {Id}, ConnectionId: {ConnectionId}, Name: {Name}, Images: {string.Join(", ", Images)}, IsReady: {IsReady}";
        }
    }
}