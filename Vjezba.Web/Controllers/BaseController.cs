// Vjezba.Web/Controllers/BaseController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vjezba.Model;

namespace Vjezba.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly UserManager<AppUser> _userManager;

        public BaseController(UserManager<AppUser> userManager = null)
        {
            _userManager = userManager;
        }

        public string UserId => _userManager?.GetUserId(User);
    }
}