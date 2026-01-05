using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading;

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


        [Authorize(Roles = "User, Admin")]
        public IActionResult Index()
        {
            Dictionary<int, Echo?> echoes = [];
            var flocks = db.Flocks
                .Include(f => f.Admin!)
                    .ThenInclude(a => a.ApplicationUser)
                .Include(f => f.Echos!)
                    .ThenInclude(e => e.User!)
                        .ThenInclude(u => u.ApplicationUser)
                .Where(fl => !fl.FlockStatus.Equals("deleted"))
                .OrderByDescending(ech => ech.DateCreated);

     
            var deletedUserId = db.Users
                .Where(u => u.UserName == "deleted")
                .Select(u => u.Id)
                .FirstOrDefault() ?? string.Empty;

            foreach (var fl in flocks)
            {
                Echo? ech = db.Echoes
                                .Include(e => e.Flock)
                                .Where(e => e.FlockId == fl.Id && !e.IsRemoved && e.UserId != deletedUserId && e.CommParentId == null)
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

            SetAccessRights();

            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult IndexAdmin()
        {
            Dictionary<int, Echo?> echoes = [];
            var flocks = db.Flocks
                .Include(f => f.Admin!)
                    .ThenInclude(a => a.ApplicationUser)
                .Include(f => f.Echos!)
                    .ThenInclude(e => e.User!)
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

            SetAccessRights();

            return View();
        }

        // New ar fi similar cu cel de la echo, numai ca poti pune banner si pfp in loc de 2 atasamente, deci trb modificata logica...
        [Authorize(Roles = "User, Admin")]
        public IActionResult New()
        {
            Flock fl = new();
            SetAccessRights();
            return View(fl);
        }

        // POST pt New, basically copiat de la Echo backend wise si cred ca nu e nevoie de altceva mai mult ca paginile vor fi very similar
        // singura diferenta ar fi ca nu avem doua inputuri si atribute random, chiar conteaza care-i care si nu ar trb sa fie interschimbabile
        // am putea pune la nivel de frontend si un preview pt profil, ca sa vada userul cum ar arata profilul lor daca l-ar edita.
        // si daca e, putem face ceva similar si pt Echo sau comment, like un div de preview pt creearea postarii ca si cum am scrie divul ala live
        // gen yk cum teitter iti ofera un fel de preview cand scrii un tweet, cv de genul sa fie si pagina aia, dar mna asta e frontend

        [Authorize(Roles = "User, Admin")]
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
                    SetAccessRights();
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
                    SetAccessRights();
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
                    var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Flocks", flock.Id.ToString());
                    Directory.CreateDirectory(directoryPath); // Create directory if it doesn't exist

                    var storagePath = Path.Combine(directoryPath, pfp.FileName);
                    var databaseFileName = "/Resources/Alex/Flocks/" + flock.Id + "/" + pfp.FileName;

                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await pfp.CopyToAsync(fileStream);
                    }

                    flock.PfpLink = databaseFileName;
                }

                if (banner != null && banner.Length > 0)
                {
                    var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Flocks", flock.Id.ToString());
                    Directory.CreateDirectory(directoryPath); // Create directory if it doesn't exist

                    var storagePath = Path.Combine(directoryPath, banner.FileName);
                    var databaseFileName = "/Resources/Alex/Flocks/" + flock.Id + "/" + banner.FileName;

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

            SetAccessRights();
            return View(flock);
        }

        // Edit va avea aceeasi chestie cu New la nivel de pagini etc. asa cum e si pt postari in sine
        [Authorize(Roles = "User, Admin")]
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
                SetAccessRights();
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
        [Authorize(Roles = "User, Admin")]
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
                    SetAccessRights();
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
                    SetAccessRights();
                    return View(flock);
                }
            }


            if (pfp != null && pfp.Length > 0)
            {
                var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Flocks", flock.Id.ToString());
                Directory.CreateDirectory(directoryPath);

                var storagePath = Path.Combine(directoryPath, pfp.FileName);
                var databaseFileName = "/Resources/Alex/Flocks/" + flock.Id + "/" + pfp.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    pfp.CopyTo(fileStream);
                }

                flock.PfpLink = databaseFileName;
            }


            if (banner != null && banner.Length > 0)
            {
                var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Flocks", flock.Id.ToString());
                Directory.CreateDirectory(directoryPath);

                var storagePath = Path.Combine(directoryPath, banner.FileName);
                var databaseFileName = "/Resources/Alex/Flocks/" + flock.Id + "/" + banner.FileName;

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
                SetAccessRights();
                return View(flock);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Flock? flock = db.Flocks.Find(id);

            if (flock is null)
            {
                TempData["flock-message"] = "Flock does not exist!";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (flock.AdminId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                // Obține utilizatorul deleted
                var deletedUser = db.Users
                    .Where(u => u.UserName == "deleted")
                    .FirstOrDefault();

                if (deletedUser == null)
                {
                    TempData["flock-message"] = "Deleted user not found!";
                    TempData["flock-type"] = "alert-danger";
                    return RedirectToAction("Index");
                }

                // Atribuie recursiv toate echo-urile principale din flock si comentariile lor la utilizatorul deleted
                // (comentariile vor fi atribuite recursiv, chiar daca au FlockId setat)
                var flockEchoes = db.Echoes
                    .Where(e => e.FlockId == id && e.UserId != deletedUser.Id && e.CommParentId == null)
                    .ToList();

                foreach (var echo in flockEchoes)
                {
                    AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);
                }
                
                db.SaveChanges();

                flock.FlockStatus = "deleted";
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteAdmin(int id, string? type)
        {
            if (User.IsInRole("Admin"))
            {
                Flock? flock = db.Flocks.Find(id);

                if (flock is null)
                {
                    TempData["flock-message"] = "Flock does not exist!";
                    TempData["flock-type"] = "alert-warning";
                    if (type == null)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("IndexAdmin");
                    }
                }
            
                // Obține utilizatorul deleted
                var deletedUser = db.Users
                    .Where(u => u.UserName == "deleted")
                    .FirstOrDefault();

                if (deletedUser == null)
                {
                    TempData["flock-message"] = "Deleted user not found!";
                    TempData["flock-type"] = "alert-danger";
                    if (type == null)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("IndexAdmin");
                    }
                }

                // Atribuie recursiv toate echo-urile principale din flock si comentariile lor la utilizatorul deleted
                // (comentariile vor fi atribuite recursiv, chiar daca au FlockId setat)
                var flockEchoes = db.Echoes
                    .Where(e => e.FlockId == id && e.UserId != deletedUser.Id && e.CommParentId == null)
                    .ToList();

                foreach (var echo in flockEchoes)
                {
                    AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);
                }

                db.SaveChanges();

                db.Flocks.Remove(flock);
                try
                {
                    db.SaveChanges();
                    TempData["flock-message"] = "Flock was deleted!";
                    TempData["flock-type"] = "alert-info";
                    if (type == null)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("IndexAdmin");
                    }
                }
                catch (DbUpdateException)
                {
                    TempData["flock-message"] = "Flock could not be deleted...";
                    TempData["flock-type"] = "alert-danger";
                    if (type == null)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("IndexAdmin");
                    }
                }
            }
            else
            {
                TempData["flock-message"] = "You are not authorized to delete this flock.";
                TempData["flock-type"] = "alert-warning";
                if (type == null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("IndexAdmin");
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult MarkAllDeletedAdmin()
        {
            if (User.IsInRole("Admin"))
            {
                var flocks = db.Flocks.Where(fl => !fl.FlockStatus.Equals("deleted")).OrderByDescending(fl => fl.DateCreated).ToList();

                if (flocks is null)
                {
                    TempData["flock-message"] = "There are no Flocks not marked as deleted!";
                    TempData["flock-type"] = "alert-warning";
                    return RedirectToAction("IndexAdmin");
                }

                var deletedUser = db.Users.Where(u => u.UserName == "deleted").FirstOrDefault();

                if (deletedUser == null)
                {
                    TempData["flock-message"] = "Deleted user not found!";
                    TempData["flock-type"] = "alert-danger";
                    return RedirectToAction("IndexAdmin");
                }

                foreach (var flock in flocks)
                {
                    var flockEchoes = db.Echoes
                                            .Where(e => e.FlockId == flock.Id && e.UserId != deletedUser.Id && e.CommParentId == null)
                                            .ToList();

                    foreach (var echo in flockEchoes)
                    {
                        AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);
                    }

                    db.SaveChanges();

                    flock.FlockStatus = "deleted";
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                        TempData["flock-flock-message"] = "Flock could not be deleted...";
                        TempData["flock-flock-type"] = "alert-danger";
                        return RedirectToAction("IndexAdmin");
                    }
                }   
            }
            else
            {
                TempData["flock-message"] = "You do not have the necessary permissions to delete all flocks.";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("IndexAdmin");
            }

            TempData["flock-message"] = "All non-\"deleted\" Flocks were marked as deleted!";
            TempData["flock-type"] = "alert-info";
            return RedirectToAction("IndexAdmin");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAllAdmin()
        {
            if (User.IsInRole("Admin"))
            {
                var flocks = db.Flocks.OrderByDescending(fl => fl.DateCreated).ToList();

                if (flocks is null)
                {
                    TempData["flock-message"] = "There are no Flocks in the database!";
                    TempData["flock-type"] = "alert-warning";
                    return RedirectToAction("IndexAdmin");
                }

                var deletedUser = db.Users.Where(u => u.UserName == "deleted").FirstOrDefault();

                if (deletedUser == null)
                {
                    TempData["flock-message"] = "Deleted user not found!";
                    TempData["flock-type"] = "alert-danger";
                    return RedirectToAction("IndexAdmin");
                }

                foreach (var flock in flocks)
                {
                    var flockEchoes = db.Echoes
                                            .Where(e => e.FlockId == flock.Id && e.UserId != deletedUser.Id && e.CommParentId == null)
                                            .ToList();

                    foreach (var echo in flockEchoes)
                    {
                        AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);
                    }

                    db.SaveChanges();

                    db.Flocks.Remove(flock);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                        TempData["flock-flock-message"] = "Flock could not be deleted...";
                        TempData["flock-flock-type"] = "alert-danger";
                        return RedirectToAction("IndexAdmin");
                    }
                }
            }
            else
            {
                TempData["flock-message"] = "You do not have the necessary permissions to delete all flocks.";
                TempData["flock-type"] = "alert-warning";
                return RedirectToAction("IndexAdmin");
            }

            TempData["flock-message"] = "All non-\"deleted\" Flocks were marked as deleted!";
            TempData["flock-type"] = "alert-info";
            return RedirectToAction("IndexAdmin");
        }



        // Show va fi ca o vizualizare a profilului unui user designwise
        // sooo ecourile afisate ar trebui sa fie date cu link catre EchoesController for obvious reasons
        [Authorize(Roles = "User, Admin")]
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

            var deletedUserId = db.Users
                .Where(u => u.UserName == "deleted")
                .Select(u => u.Id)
                .FirstOrDefault() ?? string.Empty;

            var echoes = db.Echoes // am pus si Echoes ca mna afisam practic toate postarile din comunitate
                            .Include(ech => ech.User)
                                .ThenInclude(u => u.ApplicationUser)
                            .Include(ech => ech.Interactions)
                            .Where(e => e.FlockId == id && e.CommParentId == null && !e.IsRemoved && e.UserId != deletedUserId) // Filtreaza postarile sterse
                            .OrderByDescending(ech => ech.DateCreated);

            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.FlockEchoes = echoes;

            if (TempData.ContainsKey("flock-message"))
            {
                ViewBag.Message = TempData["flock-message"];
                ViewBag.Type = TempData["flock-type"];
            }

            SetAccessRights();

            return View(flock);
        }



        // Other Methods

        private void SetAccessRights()
        {
            ViewBag.VisibleShowDelete = false;

            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.IsAdmin = User.IsInRole("Admin");

            if (User.IsInRole("Editor"))
            {
                ViewBag.VisibleShowDelete = true;
            }
        }

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

        // Atribuie echo și comentarii recursiv la utilizatorul deleted
        private void AssignEchoAndChildrenToDeletedUser(Echo echo, string deletedUserId)
        {
            echo.UserId = deletedUserId;
            echo.IsRemoved = true;
            echo.FlockId = null; // elimina FK-ul catre flock

            var comments = db.Echoes
                            .Where(ech => ech.CommParentId == echo.Id && ech.UserId != deletedUserId)
                            .ToList();

            foreach (var comment in comments)
            {
                AssignEchoAndChildrenToDeletedUser(comment, deletedUserId);
            }
        }
    }
}
