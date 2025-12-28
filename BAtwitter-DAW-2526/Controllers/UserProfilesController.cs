using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BAtwitter_DAW_2526.Controllers
{
    public class UserProfilesController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public UserProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> usrm, RoleManager<IdentityRole> rlm, IWebHostEnvironment env)
        {
            db = context;
            _roleManager = rlm;
            _userManager = usrm;
            _env = env;
        }



        // [HttpGet] care se executa implicit
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        public IActionResult Index()
        {


            Echo? ech = db.Echoes
                            .Include(e => e.UserId)
                            .Where(e => e.UserId == _userManager.GetUserId(User) && !e.IsRemoved && e.CommParentId == null)
                            .OrderByDescending(e => e.DateCreated)
                            .FirstOrDefault();

            ViewBag.Echoes = ech;

            if (TempData.ContainsKey("userprofile-message"))
            {
                ViewBag.Message = TempData["userprofile-message"];
                ViewBag.Type = TempData["userprofile-type"];
            }

            return View();
        }

       
        // Edit va avea aceeasi chestie cu New la nivel de pagini etc. asa cum e si pt postari in sine
        //[Authorize(Roles = "FlockAdmin")]
        public IActionResult Edit(int id)
        {
            UserProfile? userPf = db.UserProfiles.Where(f => f.Id == id).FirstOrDefault();

            if (flock is null)
            {
                TempData["flock-message"] = "Flock does not exist!";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (flock.AdminId == _userManager.GetUserId(User))
            {
                return View(flock);
            }
            else
            {
                TempData["flock-message"] = "You are not authorized to change the details of this flock.";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("Index");
            }
        }

        // POST pt Edit, kind of just copy pasted cu niste micute modificari ca o sa fie basically acelasi lucru at the end of the day
        //[Authorize(Roles = "FlockAdmin")]
        [HttpPost]
        public IActionResult Edit(int id, Flock reqFlock, IFormFile? pfp, IFormFile? banner, bool removePfp = false, bool removeBanner = false)
        {
            Flock? flock = db.Flocks.Find(id);

            if (flock is null)
            {
                TempData["flock-message"] = "Flock does not exist!";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (flock.AdminId != _userManager.GetUserId(User))
            {
                TempData["flock-message"] = "You are not authorized to change the details of this flock.";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            flock.Name = reqFlock.Name;
            flock.Description = reqFlock.Description;

            // Active after pressing delete and no new files
            if (removePfp && !string.IsNullOrEmpty(flock.PfpLink))
            {
                DeletePhysicalFile(flock.PfpLink);
                flock.PfpLink = null;
            }

            //  -- ADD BANNER TO FLOCK DETAILS
            if (removeBanner && !string.IsNullOrEmpty(flock.BannerLink))
            {
                DeletePhysicalFile(flock.BannerLink);
                flock.BannerLink = null;
            }


            if (pfp != null && pfp.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" }; // ".gif", ".mp4", ".mov" - PROBABLY NOT
                var fileExtension = Path.GetExtension(pfp.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PfpLink", "Flock profile picture must be an image (jpg, jpeg, png, webp, gif).");
                    return View(flock);
                }
            }

            if (banner != null && banner.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" }; // ".gif", ".mp4", ".mov" - macar sa punem gifuri? alea n-ar fi grele per se
                var fileExtension = Path.GetExtension(banner.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Banner", "Banner must be an image (jpg, jpeg, png, webp, gif)");
                    return View(flock);
                }
            }


            if (pfp != null && pfp.Length > 0)
            {
                var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Ioan", "Flocks", flock.Id.ToString());
                Directory.CreateDirectory(directoryPath);

                var storagePath = Path.Combine(directoryPath, pfp.FileName);
                var databaseFileName = "/Resources/Ioan/Flocks/" + flock.Id + "/" + pfp.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    pfp.CopyTo(fileStream);
                }

                flock.PfpLink = databaseFileName;
            }


            if (banner != null && banner.Length > 0)
            {
                var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Ioan", "Flocks", flock.Id.ToString());
                Directory.CreateDirectory(directoryPath);

                var storagePath = Path.Combine(directoryPath, banner.FileName);
                var databaseFileName = "/Resources/Ioan/Flocks/" + flock.Id + "/" + banner.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    banner.CopyTo(fileStream);
                }

                flock.BannerLink = databaseFileName; // - ADD BANNER TO FLOCK
            }

            if (TryValidateModel(flock))
            {
                db.SaveChanges();

                TempData["flock-message"] = "Flock was modified succesfully!";
                TempData["flock-type"] = "alert-info";
                return RedirectToAction("Show", new { id = flock.Id });
            }
            else
            {
                return View(flock);
            }
        }

        // Delete e doar o metoda POST, nu are pagina routed
        //[Authorize(Roles = "FlockAdmin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Flock? flock = db.Flocks.Find(id);

            if (flock is null)
            {
                TempData["flock-message"] = "Flock does not exist!";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (flock.AdminId == _userManager.GetUserId(User))
            {
                // Sterge recursiv toate echo-urile principale din flock si comentariile lor
                // (comentariile vor fi sterse recursiv, chiar daca au FlockId setat)
                var flockEchoes = db.Echoes
                    .Where(e => e.FlockId == id && !e.IsRemoved && e.CommParentId == null)
                    .ToList();

                foreach (var echo in flockEchoes)
                {
                    MarkEchoAndChildrenAsRemoved(echo);
                }

                db.SaveChanges();


                db.Flocks.Remove(flock);

                try
                {
                    db.SaveChanges();
                    TempData["flock-message"] = "Flock was deleted!";
                    TempData["flock-type"] = "alert-info";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    TempData["flock-message"] = "Flock could not be deleted...";
                    TempData["flock-type"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["flock-message"] = "You are not authorized to delete this flock.";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("Index");
            }
        }


        // Show va fi ca o vizualizare a profilului unui user designwise
        // sooo ecourile afisate ar trebui sa fie date cu link catre EchoesController for obvious reasons
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        public IActionResult Show(int id)
        {
            var flock = db.Flocks
                .Include(f => f.Admin)
                    .ThenInclude(a => a.ApplicationUser)
                .Where(f => f.Id == id)
                .FirstOrDefault();


            if (flock is null)
            {
                TempData["flock-message"] = "Flock does not exist!";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            var echoes = db.Echoes // am pus si Echoes ca mna afisam practic toate postarile din comunitate
                            .Include(ech => ech.User)
                                .ThenInclude(u => u.ApplicationUser)
                            .Include(ech => ech.Interactions)
                            .Where(e => e.FlockId == id && e.CommParentId == null && !e.IsRemoved) // Filtreaza postarile sterse
                            .OrderByDescending(ech => ech.DateCreated);

            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.FlockEchoes = echoes;

            if (TempData.ContainsKey("flock-message"))
            {
                ViewBag.Message = TempData["flock-message"];
                ViewBag.Type = TempData["flock-type"];
            }

            return View(flock);
        }



        // Other Methods
        private void DeletePhysicalFile(string relativePath)
        {
            var physicalPath = Path.Combine(
                _env.WebRootPath,
                relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        // Stergere echo si comentarii recursiv
        private void MarkEchoAndChildrenAsRemoved(Echo echo)
        {
            echo.IsRemoved = true;
            echo.FlockId = null; // elimina FK-ul catre flock

            var comments = db.Echoes
                            .Where(ech => ech.CommParentId == echo.Id && !ech.IsRemoved)
                            .ToList();

            foreach (var comment in comments)
            {
                MarkEchoAndChildrenAsRemoved(comment);
            }
        }
    
    }
}
