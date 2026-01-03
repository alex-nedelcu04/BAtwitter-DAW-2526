using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BAtwitter_DAW_2526.Controllers
{
    public class InteractionsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public InteractionsController(ApplicationDbContext context, UserManager<ApplicationUser> usrm, RoleManager<IdentityRole> rlm, IWebHostEnvironment env)
        {
            db = context;
            _roleManager = rlm;
            _userManager = usrm;
            _env = env;
        }

        public IActionResult New_Rebound()
        {


            return View();
        }
    }
}
