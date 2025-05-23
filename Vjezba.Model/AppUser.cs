using System.ComponentModel.DataAnnotations; // Dodaj ovo
using Microsoft.AspNetCore.Identity;

namespace Vjezba.Model
{
    public class AppUser : IdentityUser
    {
        [StringLength(11, MinimumLength = 11, ErrorMessage = "OIB must be exactly 11 characters")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "OIB must contain only numbers")]
        public string? OIB { get; set; }

        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG must be exactly 13 characters")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "JMBG must contain only numbers")]
        public string? JMBG { get; set; }
    }
}
