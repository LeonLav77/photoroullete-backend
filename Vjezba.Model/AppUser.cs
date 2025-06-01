using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Vjezba.Model
{
    public class AppUser : IdentityUser
    {
        [Display(Name = "Are you going to invite people to games?")]
        public bool WillInvitePlayers { get; set; } = false;
    }
}