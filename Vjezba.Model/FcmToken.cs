using System.ComponentModel.DataAnnotations;

namespace Vjezba.Model
{
    public class FcmToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            return $"FCM Token: {Token}";
        }
    }
}