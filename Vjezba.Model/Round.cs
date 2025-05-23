using System.ComponentModel.DataAnnotations;

namespace Vjezba.Model
{
    public class Round
    {
        [Key]
        public int Id { get; set; }

        public int Number { get; set; }

        public int Duration { get; set; }

        public string Image { get; set; }

        public string CorrectAnswer { get; set; } = string.Empty;

        public List<Answer> Answers { get; set; } = new List<Answer>();

        // Foreign key for Game
        public int GameId { get; set; }
        public virtual Game Game { get; set; }

        // Navigation property
        public virtual ICollection<Answer> AnswersCollection { get; set; } = new List<Answer>();
    }
}