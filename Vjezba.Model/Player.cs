using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vjezba.Model
{
    public class Player
    {
        [Key]
        public int Id { get; set; }

        public string ConnectionId { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public List<string> Images { get; set; } = new List<string>();

        public string ImagesJson { get; set; } = "[]";

        public bool IsReady { get; set; } = false;

        public int? GameId { get; set; }
        public virtual Game Game { get; set; }

        public Player() { }

        public Player(string connectionId, string name)
        {
            ConnectionId = connectionId;
            Name = name;
        }

        [NotMapped]
        public List<string> ImagesProperty
        {
            get
            {
                if (string.IsNullOrEmpty(ImagesJson))
                    return new List<string>();
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(ImagesJson) ?? new List<string>();
            }
            set
            {
                Images = value;
                ImagesJson = System.Text.Json.JsonSerializer.Serialize(value);
            }
        }

        public override string ToString()
        {
            return $"Id: {Id}, ConnectionId: {ConnectionId}, Name: {Name}, Images: {string.Join(", ", Images)}, IsReady: {IsReady}";
        }
    }
}