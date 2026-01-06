using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BAtwitter_DAW_2526.Areas.Identity.Pages.Account;

namespace BAtwitter_DAW_2526.Controllers
{
    public class UserProfilesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public UserProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> usrm, RoleManager<IdentityRole> rlm, IWebHostEnvironment env, SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            db = context;
            _roleManager = rlm;
            _userManager = usrm;
            _env = env;

            _signInManager = signInManager;
            _logger = logger;
        }

        // Index ar trb sa fie profilul utilizatorului curent (sau redirect la Show cu username-ul curent)
        // [HttpGet] care se executa implicit
        [Authorize(Roles = "User, Admin")]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", "Identity");
            }

            var currentUser = db.Users.Find(currentUserId);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", "Identity" );
            }

            return RedirectToAction("Show", new { username = currentUser.UserName });
        }

        // New nu e necesar pentru UserProfile
        // Dacă UserProfile nu există, redirect la Register
        [Authorize(Roles = "User, Admin")]
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
            return RedirectToAction("Register", "Account", "Identity");
        }


        [Authorize(Roles = "User, Admin")]
        public IActionResult Edit(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", "Identity");
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
                SetAccessRights();
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
        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(string id, UserProfile reqUserProfile, IFormFile? pfp, IFormFile? banner, bool removePfp = false, bool removeBanner = false)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", "Identity");
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
            userProfile.AccountStatus = reqUserProfile.AccountStatus;

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
                    SetAccessRights();
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
                    SetAccessRights();
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
                SetAccessRights();
                return View(userProfile);
            }
        }

        // Delete pentru UserProfile - probabil nu ar trebui să fie disponibil sau ar trebui să fie diferit
        // Poate doar să marcheze contul ca "deleted" sau "suspended" în loc să șteargă efectiv
        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", "Identity");
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
                // Obține utilizatorul deleted
                var deletedUser = db.Users
                    .Where(u => u.UserName == "deleted")
                    .FirstOrDefault();

                if (deletedUser == null)
                {
                    TempData["userprofile-message"] = "System error: deleted user not found!";
                    TempData["userprofile-type"] = "alert-danger";
                    return RedirectToAction("Index");
                }

                // Marchează contul ca șters
                userProfile.AccountStatus = "deleted";

                // Atribuie toate echo-urile utilizatorului la utilizatorul deleted
                var userEchoes = db.Echoes
                    .Where(e => e.UserId == id && e.UserId != deletedUser.Id && e.CommParentId == null)
                    .ToList();

                foreach (var echo in userEchoes)
                {
                    AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);
                }

                try
                {
                    var user = await _userManager.GetUserAsync(User);

                    if (user == null)
                    {
                        DbUpdateException a = new();
                        throw a;
                    }

                    db.UserProfiles.Remove(userProfile);

                    await _userManager.DeleteAsync(user);
                    await _signInManager.SignOutAsync();
                    _logger.LogInformation("User logged out.");

                    await db.SaveChangesAsync();

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
        [Authorize(Roles = "User, Admin")]
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

            // Obține ID-ul utilizatorului deleted pentru a-l exclude
            var deletedUserId = db.Users
                .Where(u => u.UserName == "deleted")
                .Select(u => u.Id)
                .FirstOrDefault() ?? string.Empty;

            var echoes = db.Echoes
                            .Include(ech => ech.User)
                                .ThenInclude(u => u.ApplicationUser)
                            .Include(ech => ech.Interactions)
                            .Where(e => e.UserId == userProfile.Id && e.UserId != deletedUserId && e.CommParentId == null && !e.IsRemoved) // Filtreaza postarile sterse
                            .OrderByDescending(ech => ech.DateCreated);

            var currentUserId = _userManager.GetUserId(User);
            ViewBag.CurrentUser = currentUserId;
            ViewBag.UserEchoes = echoes;

            // Check follow request status
            if (!string.IsNullOrEmpty(currentUserId) && currentUserId != userProfile.Id)
            {
                // Check if already following
                var isFollowing = db.Relations
                    .Any(r => r.SenderId == currentUserId && r.ReceiverId == userProfile.Id && r.Type == 1);
                
                // Check if there's a pending request
                var pendingRequest = db.FollowRequests
                    .FirstOrDefault(fr => fr.SenderUserId == currentUserId && 
                                        fr.ReceiverUserId == userProfile.Id && 
                                        fr.ReceiverFlockId == null);
                
                ViewBag.IsFollowing = isFollowing;
                ViewBag.HasPendingRequest = pendingRequest != null;
                ViewBag.IsPrivateAccount = userProfile.AccountStatus == "private";
            }
            else
            {
                ViewBag.IsFollowing = false;
                ViewBag.HasPendingRequest = false;
                ViewBag.IsPrivateAccount = false;
            }

            if (TempData.ContainsKey("userprofile-message"))
            {
                ViewBag.Message = TempData["userprofile-message"];
                ViewBag.Type = TempData["userprofile-type"];
            }

            SetAccessRights();

            return View(userProfile);
        }



        [Authorize(Roles = "Admin")]
        public IActionResult IndexAdmin()
        {
            var userProfiles = db.UserProfiles
                .Include(up => up.ApplicationUser)
                .OrderByDescending(up => up.JoinDate);

            ViewBag.UserProfiles = userProfiles;

            if (TempData.ContainsKey("userprofile-message"))
            {
                ViewBag.Message = TempData["userprofile-message"];
                ViewBag.Type = TempData["userprofile-type"];
            }

            SetAccessRights();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult MarkAsDeletedAdmin(string id)
        {
            if (User.IsInRole("Admin"))
            {
                UserProfile? userProfile = db.UserProfiles
                    .Include(up => up.ApplicationUser)
                    .Where(up => up.Id == id)
                    .FirstOrDefault();

                if (userProfile is null)
                {
                    TempData["userprofile-message"] = "User profile does not exist!";
                    TempData["userprofile-type"] = "alert-warning";
                    return RedirectToAction("IndexAdmin");
                }

                var deletedUser = db.Users
                    .Where(u => u.UserName == "deleted")
                    .FirstOrDefault();

                if (deletedUser == null)
                {
                    TempData["userprofile-message"] = "Deleted user not found!";
                    TempData["userprofile-type"] = "alert-danger";
                    return RedirectToAction("IndexAdmin");
                }

                // Marchează contul ca șters
                userProfile.AccountStatus = "deleted";

                // Atribuie toate echo-urile utilizatorului la utilizatorul deleted
                var userEchoes = db.Echoes
                    .Where(e => e.UserId == id && e.UserId != deletedUser.Id && e.CommParentId == null)
                    .ToList();

                foreach (var echo in userEchoes)
                {
                    AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);
                }

                var username = userProfile.ApplicationUser?.UserName;
                var referer = Request.Headers["Referer"].ToString();
                var isFromShow = referer.Contains("UserProfiles/Show");

                try
                {
                    db.SaveChanges();
                    TempData["userprofile-message"] = "User profile was marked as deleted!";
                    TempData["userprofile-type"] = "alert-info";
                    
                    if (isFromShow && !string.IsNullOrEmpty(username))
                    {
                        return RedirectToAction("Show", new { username = username });
                    }
                    return RedirectToAction("IndexAdmin");
                }
                catch (DbUpdateException)
                {
                    TempData["userprofile-message"] = "User profile could not be marked as deleted...";
                    TempData["userprofile-type"] = "alert-danger";
                    
                    if (isFromShow && !string.IsNullOrEmpty(username))
                    {
                        return RedirectToAction("Show", new { username = username });
                    }
                    return RedirectToAction("IndexAdmin");
                }
            }
            else
            {
                TempData["userprofile-message"] = "You do not have the necessary permissions to mark this user profile as deleted.";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("IndexAdmin");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            if (User.IsInRole("Admin"))
            {
                UserProfile? userProfile = db.UserProfiles
                    .Include(up => up.ApplicationUser)
                    .Where(up => up.Id == id)
                    .FirstOrDefault();

                if (userProfile is null)
                {
                    TempData["userprofile-message"] = "User profile does not exist!";
                    TempData["userprofile-type"] = "alert-warning";
                    return RedirectToAction("IndexAdmin");
                }

                var username = userProfile.ApplicationUser?.UserName;
                var referer = Request.Headers["Referer"].ToString();
                var isFromShow = referer.Contains("UserProfiles/Show");

                // Get deleted user for reassigning echoes
                var deletedUser = db.Users
                    .Where(u => u.UserName == "deleted")
                    .FirstOrDefault();

                if (deletedUser == null)
                {
                    TempData["userprofile-message"] = "Deleted user not found!";
                    TempData["userprofile-type"] = "alert-danger";
                    return RedirectToAction("IndexAdmin");
                }

                try
                {
              

                    var flockUsers = db.FlockUsers.Where(fu => fu.UserId == id).ToList();
                    db.FlockUsers.RemoveRange(flockUsers);

                    var interactions = db.Interactions.Where(i => i.UserId == id).ToList();
                    db.Interactions.RemoveRange(interactions);

                    var sentRelations = db.Relations.Where(r => r.SenderId == id).ToList();
                    var receivedRelations = db.Relations.Where(r => r.ReceiverId == id).ToList();
                    db.Relations.RemoveRange(sentRelations);
                    db.Relations.RemoveRange(receivedRelations);

                    var adminFlocks = db.Flocks.Where(f => f.AdminId == id).ToList();
                    foreach (var flock in adminFlocks)
                    {
                        var flockEchoes = db.Echoes
                            .Where(e => e.FlockId == flock.Id && e.UserId != deletedUser.Id && e.CommParentId == null)
                            .ToList();

                        foreach (var echo in flockEchoes)
                        {
                            AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);
                        }

                        await db.SaveChangesAsync();

                        db.Flocks.Remove(flock);
                    }
                    var userEchoes = db.Echoes
                        .Where(e => e.UserId == id && e.UserId != deletedUser.Id && e.CommParentId == null)
                        .ToList();

                    foreach (var echo in userEchoes)
                    {
                        AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);
                    }

                    await db.SaveChangesAsync();

                    db.UserProfiles.Remove(userProfile);
                    await db.SaveChangesAsync();

                    var applicationUser = await _userManager.FindByIdAsync(id);
                    if (applicationUser != null)
                    {
                        await _userManager.DeleteAsync(applicationUser);
                    }

                    if (!string.IsNullOrEmpty(userProfile.PfpLink) && !userProfile.PfpLink.Contains("user_default_pfp"))
                    {
                        DeletePhysicalFile(userProfile.PfpLink);
                    }

                    if (!string.IsNullOrEmpty(userProfile.BannerLink) && !userProfile.BannerLink.Contains("banner_default"))
                    {
                        DeletePhysicalFile(userProfile.BannerLink);
                    }

                    var userDirectory = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Users", userProfile.Id);
                    if (Directory.Exists(userDirectory))
                    {
                        try
                        {
                            Directory.Delete(userDirectory, recursive: true);
                        }
                        catch (DirectoryNotFoundException)
                        {
                        }
                        catch (IOException ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Could not delete directory {userDirectory}: {ex.Message}");
                        }
                    }

                    TempData["userprofile-message"] = "User profile was permanently deleted!";
                    TempData["userprofile-type"] = "alert-info";
                    

                    return RedirectToAction("IndexAdmin");
                }
                catch (DbUpdateException ex)
                {
                    TempData["userprofile-message"] = $"User profile could not be deleted: {ex.Message}";
                    TempData["userprofile-type"] = "alert-danger";
                    
                    if (isFromShow && !string.IsNullOrEmpty(username))
                    {
                        return RedirectToAction("Show", new { username = username });
                    }
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["userprofile-message"] = $"An error occurred while deleting user profile: {ex.Message}";
                    TempData["userprofile-type"] = "alert-danger";
                    
                    if (isFromShow && !string.IsNullOrEmpty(username))
                    {
                        return RedirectToAction("Show", new { username = username });
                    }
                    return RedirectToAction("IndexAdmin");
                }
            }
            else
            {
                TempData["userprofile-message"] = "You do not have the necessary permissions to delete this user profile.";
                TempData["userprofile-type"] = "alert-warning";
                return RedirectToAction("IndexAdmin");
            }
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
