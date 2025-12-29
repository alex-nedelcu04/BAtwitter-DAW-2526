using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
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

        // Index ar trb sa fie profilul utilizatorului curent (sau redirect la Show cu username-ul curent)
        // [HttpGet] care se executa implicit
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var currentUser = db.Users.Find(currentUserId);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            return RedirectToAction("Show", new { username = currentUser.UserName });
        }

        // New nu e necesar pentru UserProfile
        // Dacă UserProfile nu există, redirect la Register
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        public IActionResult New()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Register", "Account", new { area = "Identity" });
            }

            // Verifica daca UserProfile exista deja
            var existingProfile = db.UserProfiles.Find(currentUserId);
            if (existingProfile != null)
            {
                var currentUser = db.Users.Find(currentUserId);
                TempData["userprofile-message"] = "Your profile already exists!";
                TempData["userprofile-type"] = "alert-info";
                return RedirectToAction("Show", new { username = currentUser?.UserName });
            }

            // Daca nu exista => redirect la Register
            return RedirectToAction("Register", "Account", new { area = "Identity" });
        }

       
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        public IActionResult Edit(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            UserProfile? userProfile = db.UserProfiles
                .Include(up => up.ApplicationUser)
                .Where(up => up.Id == id)
                .FirstOrDefault();

            if (userProfile is null)
            {
                TempData["userprofile-message"] = "User profile does not exist!";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (userProfile.Id == currentUserId)
            {
                return View(userProfile);
            }
            else
            {
                TempData["userprofile-message"] = "You are not authorized to change the details of this profile.";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("Show", new { username = userProfile.ApplicationUser?.UserName });
            }
        }

        // POST pt Edit, kind of just copy pasted cu niste micute modificari ca o sa fie basically acelasi lucru at the end of the day
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        [HttpPost]
        public async Task<IActionResult> Edit(string id, UserProfile reqUserProfile, IFormFile? pfp, IFormFile? banner, bool removePfp = false, bool removeBanner = false)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            UserProfile? userProfile = db.UserProfiles.Find(id);

            if (userProfile is null)
            {
                TempData["userprofile-message"] = "User profile does not exist!";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (userProfile.Id != currentUserId)
            {
                // Get the username from the Users table since ApplicationUser navigation property wasn't loaded
                var applicationUser = db.Users.Find(userProfile.Id);
                var username = applicationUser?.UserName;

                TempData["userprofile-message"] = "You are not authorized to change the details of this profile.";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("Show", new { username = username });
            }

            userProfile.DisplayName = reqUserProfile.DisplayName;
            userProfile.Description = reqUserProfile.Description;
            userProfile.Pronouns = reqUserProfile.Pronouns;

            // Active after pressing delete and no new files
            if (removePfp && !string.IsNullOrEmpty(userProfile.PfpLink))
            {
                // Nu șterge default-ul
                if (!userProfile.PfpLink.Contains("user_default_pfp"))
                {
                    DeletePhysicalFile(userProfile.PfpLink);
                }
                userProfile.PfpLink = "/Resources/Images/user_default_pfp.jpg";
            }

            if (removeBanner && !string.IsNullOrEmpty(userProfile.BannerLink))
            {
                // Nu șterge default-ul
                if (!userProfile.BannerLink.Contains("banner_default"))
                {
                    DeletePhysicalFile(userProfile.BannerLink);
                }
                userProfile.BannerLink = "/Resources/Images/banner_default.jpg";
            }

            if (pfp != null && pfp.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(pfp.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PfpLink", "Profile picture must be an image (jpg, jpeg, png, webp).");
                    return View(userProfile);
                }
            }

            if (banner != null && banner.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(banner.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("BannerLink", "Banner must be an image (jpg, jpeg, png, webp)");
                    return View(userProfile);
                }
            }

            if (pfp != null && pfp.Length > 0)
            {
                // Șterge vechiul fișier dacă nu e default
                if (!string.IsNullOrEmpty(userProfile.PfpLink) && !userProfile.PfpLink.Contains("user_default_pfp"))
                {
                    DeletePhysicalFile(userProfile.PfpLink);
                }

                var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Users", userProfile.Id);
                Directory.CreateDirectory(directoryPath);

                var safeName = Path.GetFileName(pfp.FileName);
                var storagePath = Path.Combine(directoryPath, safeName);
                var databaseFileName = "/Resources/Alex/Users/" + userProfile.Id + "/" + safeName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await pfp.CopyToAsync(fileStream);
                }

                userProfile.PfpLink = databaseFileName;
            }

            if (banner != null && banner.Length > 0)
            {
                // Șterge vechiul fișier dacă nu e default
                if (!string.IsNullOrEmpty(userProfile.BannerLink) && !userProfile.BannerLink.Contains("banner_default"))
                {
                    DeletePhysicalFile(userProfile.BannerLink);
                }

                var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Users", userProfile.Id);
                Directory.CreateDirectory(directoryPath);

                var safeName = Path.GetFileName(banner.FileName);
                var storagePath = Path.Combine(directoryPath, safeName);
                var databaseFileName = "/Resources/Alex/Users/" + userProfile.Id + "/" + safeName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await banner.CopyToAsync(fileStream);
                }

                userProfile.BannerLink = databaseFileName;
            }

            if (TryValidateModel(userProfile))
            {
                await db.SaveChangesAsync();

                // Get the username from the Users table since ApplicationUser navigation property wasn't loaded
                var applicationUser = db.Users.Find(userProfile.Id);
                var username = applicationUser?.UserName;

                TempData["userprofile-message"] = "Profile was modified successfully!";
                TempData["userprofile-type"] = "alert-info";
                return RedirectToAction("Show", new { username = username });
            }
            else
            {
                return View(userProfile);
            }
        }

        // Delete pentru UserProfile - probabil nu ar trebui să fie disponibil sau ar trebui să fie diferit
        // Poate doar să marcheze contul ca "deleted" sau "suspended" în loc să șteargă efectiv
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            UserProfile? userProfile = db.UserProfiles.Find(id);

            if (userProfile is null)
            {
                TempData["userprofile-message"] = "User profile does not exist!";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (userProfile.Id == currentUserId)
            {
                // Marchează contul ca șters în loc să-l ștergi efectiv
                // Sau poți șterge UserProfile dacă e necesar, dar ApplicationUser va rămâne
                userProfile.AccountStatus = "deleted";

                // Marchează toate echo-urile utilizatorului ca șterse
                var userEchoes = db.Echoes
                    .Where(e => e.UserId == id && !e.IsRemoved && e.CommParentId == null)
                    .ToList();

                foreach (var echo in userEchoes)
                {
                    MarkEchoAndChildrenAsRemoved(echo);
                }

                try
                {
                    db.SaveChanges();

                    TempData["userprofile-message"] = "Account was deleted!";
                    TempData["userprofile-type"] = "alert-info";
                    return RedirectToAction("Index", "Home");
                }
                catch (DbUpdateException)
                {
                    // Get the username from the Users table since ApplicationUser navigation property wasn't loaded
                    var applicationUser = db.Users.Find(userProfile.Id);
                    var username = applicationUser?.UserName;

                    TempData["userprofile-message"] = "Account could not be deleted...";
                    TempData["userprofile-type"] = "alert-danger";
                    return RedirectToAction("Show", new { username = username });
                }
            }
            else
            {
                // Get the username from the Users table since ApplicationUser navigation property wasn't loaded
                var applicationUser = db.Users.Find(userProfile.Id);
                var username = applicationUser?.UserName;

                TempData["userprofile-message"] = "You are not authorized to delete this profile.";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("Show", new { username = username });
            }
        }


        // Show va fi ca o vizualizare a profilului unui user designwise
        // sooo ecourile afisate ar trebui sa fie date cu link catre EchoesController for obvious reasons
        // [Authorize(Roles = "FlockUser, FlockModerator, FlockAdmin")]
        [Route("UserProfiles/Show/{username}")]
        public IActionResult Show(string username)
        {
            // If no username is provided, redirect to Index to get current user's profile
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index");
            }

            // Find user by username first, then get their profile
            var applicationUser = db.Users
                .Where(u => u.UserName == username)
                .FirstOrDefault();

            if (applicationUser == null)
            {
                TempData["userprofile-message"] = "User profile does not exist!";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            var userProfile = db.UserProfiles
                .Include(up => up.ApplicationUser)
                .Where(up => up.Id == applicationUser.Id)
                .FirstOrDefault();

            if (userProfile is null)
            {
                TempData["userprofile-message"] = "User profile does not exist!";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            var echoes = db.Echoes
                            .Include(ech => ech.User)
                                .ThenInclude(u => u.ApplicationUser)
                            .Include(ech => ech.Interactions)
                            .Where(e => e.UserId == userProfile.Id && e.CommParentId == null && !e.IsRemoved) // Filtreaza postarile sterse
                            .OrderByDescending(ech => ech.DateCreated);

            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.UserEchoes = echoes;

            if (TempData.ContainsKey("userprofile-message"))
            {
                ViewBag.Message = TempData["userprofile-message"];
                ViewBag.Type = TempData["userprofile-type"];
            }

            return View(userProfile);
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
            // Nu elimin UserId pentru că echo-urile rămân asociate cu utilizatorul chiar dacă sunt marcate ca șterse

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
