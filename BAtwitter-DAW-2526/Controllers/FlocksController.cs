using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BAtwitter_DAW_2526.Controllers
{
    public class FlocksController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        
        public FlocksController(ApplicationDbContext context, UserManager<ApplicationUser> usrm, RoleManager<IdentityRole> rlm, IWebHostEnvironment env)
        {
            db = context;
            _roleManager = rlm;
            _userManager = usrm;
            _env = env;
        }

        // nu am modificat modelul Flock si sa fac migratie ca mi-e frica sa nu stric ceva :)))
        // dar ar trb pus un string pt Banner (daca il pui tu denumeste-l Banner ca sa nu schimbi numele aici)
        // daca e ar merge si un dateEdited? dar n-am avea unde sa-l afisam so nu prea are sens
        // si mna si UserProfile are nevoie de Banner
        // VEZI CAND BAGI BANNER SA DECOMENTEZI CHESTIILE LEGATE DE BANNER DE AICI CA DAU EROARE RN SI D-AIA LE-AM COMENTAT

        // Index ar trb sa fie lista tuturor flockurilor din aplicatie
        // ca un feed: cea mai noua postare + banner pt flockul respectiv deasupra postarii in stilul viewului de profil ca un fel de advertisment
        // [HttpGet] care se executa implicit
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        public IActionResult Index()
        {
            Dictionary<int, Echo?> echoes = [];
            var flocks = db.Flocks
                .Include(f => f.Admin)
                    .ThenInclude(a => a.ApplicationUser)
                .Include(f => f.Echos)
                    .ThenInclude(e => e.User)
                        .ThenInclude(u => u.ApplicationUser)
                .OrderByDescending(ech => ech.DateCreated);

            foreach (var fl in flocks)
            {
                Echo? ech = db.Echoes
                                .Include(e => e.Flock)
                                .Where(e => e.FlockId == fl.Id && !e.IsRemoved && e.CommParentId == null)
                                .OrderByDescending(e => e.DateCreated)
                                .FirstOrDefault();

                echoes.TryAdd(fl.Id, ech);
            }

            ViewBag.Flocks = flocks;
            ViewBag.EchoMap = echoes;

            if (TempData.ContainsKey("flock-message"))
            {
                ViewBag.Message = TempData["flock-message"];
                ViewBag.Type = TempData["flock-type"];
            }

            return View();
        }

        // New ar fi similar cu cel de la echo, numai ca poti pune banner si pfp in loc de 2 atasamente, deci trb modificata logica...
        // [HttpGet] care se executa implicit
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        public IActionResult New()
        {
            Flock fl = new();
            return View(fl);
        }

        // POST pt New, basically copiat de la Echo backend wise si cred ca nu e nevoie de altceva mai mult ca paginile vor fi very similar
        // singura diferenta ar fi ca nu avem doua inputuri si atribute random, chiar conteaza care-i care si nu ar trb sa fie interschimbabile
        // am putea pune la nivel de frontend si un preview pt profil, ca sa vada userul cum ar arata profilul lor daca l-ar edita.
        // si daca e, putem face ceva similar si pt Echo sau comment, like un div de preview pt creearea postarii ca si cum am scrie divul ala live
        // gen yk cum teitter iti ofera un fel de preview cand scrii un tweet, cv de genul sa fie si pagina aia, dar mna asta e frontend
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        [HttpPost]
        public async Task<IActionResult> New(Flock flock, IFormFile? pfp, IFormFile? banner)
        {
            flock.DateCreated = DateTime.Now;
            flock.AdminId = _userManager.GetUserId(User) ?? string.Empty;

            // Validate file extensions before saving
            if (pfp != null && pfp.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" }; 
                var fileExtension = Path.GetExtension(pfp.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PfpLink", "Flock profile picture must be an image (jpg, jpeg, png, webp).");
                    return View(flock);
                }
            }

            if (banner != null && banner.Length > 0)
            {
                // chestia e ca ar merge maxim un gif ca ar trb sa dam autoplay si loop la videos
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" }; // ".gif", ".mp4", ".mov"
                var fileExtension = Path.GetExtension(banner.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Banner", "Banner must be an image (jpg, jpeg, png, webp).");
                    return View(flock);
                }
            }

            if (TryValidateModel(flock))
            {
                db.Flocks.Add(flock);
                await db.SaveChangesAsync();

                // Now save files using the echo ID
                if (pfp != null && pfp.Length > 0)
                {
                    var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Ioan", "Flocks", flock.Id.ToString());
                    Directory.CreateDirectory(directoryPath); // Create directory if it doesn't exist

                    var storagePath = Path.Combine(directoryPath, pfp.FileName);
                    var databaseFileName = "/Resources/Ioan/Flocks/" + flock.Id + "/" + pfp.FileName;

                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await pfp.CopyToAsync(fileStream);
                    }

                    flock.PfpLink = databaseFileName;
                }

                if (banner != null && banner.Length > 0)
                {
                    var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Ioan", "Flocks", flock.Id.ToString());
                    Directory.CreateDirectory(directoryPath); // Create directory if it doesn't exist

                    var storagePath = Path.Combine(directoryPath, banner.FileName);
                    var databaseFileName = "/Resources/Ioan/Flocks/" + flock.Id + "/" + banner.FileName;

                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await banner.CopyToAsync(fileStream);
                    }

                    flock.BannerLink = databaseFileName;
                }

                // Update flock with file paths if files were uploaded
                if (pfp != null || banner != null)
                {
                    await db.SaveChangesAsync();
                }

                TempData["flock-message"] = "Flock was created succesfully!";
                TempData["flock-type"] = "alert-success";

                return RedirectToAction("Index");
            }

            return View(flock);
        }

        // Edit va avea aceeasi chestie cu New la nivel de pagini etc. asa cum e si pt postari in sine
        //[Authorize(Roles = "FlockAdmin")]
        public IActionResult Edit(int id)
        {
            Flock? flock = db.Flocks.Where(f => f.Id == id).FirstOrDefault();

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
                            .Where(e => e.FlockId == id && e.CommParent == null)
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
    }
}
