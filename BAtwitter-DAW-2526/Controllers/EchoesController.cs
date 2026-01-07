using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BAtwitter_DAW_2526.Controllers
{
    public class EchoesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public EchoesController(ApplicationDbContext context, UserManager<ApplicationUser> usrm, RoleManager<IdentityRole> rlm, IWebHostEnvironment env)
        {
            db = context;
            _roleManager = rlm;
            _userManager = usrm;
            _env = env;
        }
        // [HttpGet] care se executa implicit
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            var deletedUserId = db.Users
                .Where(u => u.UserName == "deleted")
                .Select(u => u.Id)
                .FirstOrDefault() ?? string.Empty;

            var currentUserId = _userManager.GetUserId(User);

            var followedUserIds = db.Relations
                                .Where(r => r.SenderId == currentUserId && r.Type == 1)
                                .Select(r => r.ReceiverId)
                                .ToList();

            var activeUserIds = db.UserProfiles
                                .Where(u => u.AccountStatus == "active")
                                .Select(u => u.Id)
                                .ToList();

            // Lista cu utilizatorii privați pe care NU îi urmărești
            var privateUserIdsNotFollowed = db.UserProfiles
                                .Where(u => u.AccountStatus == "private" && !followedUserIds.Contains(u.Id) && u.Id != currentUserId)
                                .Select(u => u.Id)
                                .ToList();

            // Echo-uri normale (nu sunt comentarii)
            var normalEchoes = db.Echoes
                                .Where(ech => !ech.IsRemoved && ech.UserId != deletedUserId && ech.CommParentId == null 
                                        && (ech.UserId == currentUserId || activeUserIds.Contains(ech.UserId) || followedUserIds.Contains(ech.UserId)))
                                .Include(ech => ech.User)
                                    .ThenInclude(u => u!.ApplicationUser)
                                .Include(ech => ech.AmpParent)
                                    .ThenInclude(ech => ech!.User)
                                        .ThenInclude(u => u!.ApplicationUser)
                                .Include(ech => ech.Interactions!)
                                    .ThenInclude(i => i.User)
                                        .ThenInclude(u => u!.ApplicationUser)
                                .Include(ech => ech.Flock)
                                .ToList();

            // lista care include echo-urile normale și echo-urile cu rebound-uri
            var feedItems = new List<(Echo Echo, UserProfile? ReboundUser)>();

  
            foreach (var echo in normalEchoes)
            {
                feedItems.Add((echo, null));
            }

            // echo-uri cu rebound + info user care a facut rebound
            // NU arăta rebound-uri făcute de utilizatori privați pe care nu îi urmezi (ex: foden47)
            var reboundInteractions = db.Interactions
                .Where(i => i.Rebounded &&
                            i.Echo != null &&
                            !i.Echo.IsRemoved &&
                            i.Echo.UserId != deletedUserId &&
                            i.Echo.CommParentId == null &&
                            !privateUserIdsNotFollowed.Contains(i.UserId) &&
                            (i.UserId == currentUserId || 
                             i.Echo.UserId == currentUserId || 
                             followedUserIds.Contains(i.Echo.UserId) || 
                             followedUserIds.Contains(i.UserId)))
                .Include(i => i.Echo!)
                    .ThenInclude(e => e!.User)
                        .ThenInclude(u => u!.ApplicationUser)
                .Include(i => i.Echo!.AmpParent)
                    .ThenInclude(ech => ech!.User)
                        .ThenInclude(u => u!.ApplicationUser)
                .Include(i => i.Echo!.Interactions!)
                    .ThenInclude(inter => inter.User)
                        .ThenInclude(u => u!.ApplicationUser)
                .Include(i => i.Echo!.Flock)
                .Include(i => i.User!)
                    .ThenInclude(u => u!.ApplicationUser)
                .OrderByDescending(i => i.ReboundedDate)
                .ToList();

            foreach (var interaction in reboundInteractions)
            {
                if (interaction.Echo != null && interaction.User != null)
                {
                    feedItems.Add((interaction.Echo, interaction.User));
                }
            }

            // sortare dupa data de creare a echo-ului sau data rebound-ului (dacă e rebound)
            var sortedFeedItems = feedItems
                .OrderByDescending(item => item.ReboundUser != null
                    ? item.Echo.Interactions?.FirstOrDefault(i => i.UserId == item.ReboundUser.Id && i.Rebounded)?.ReboundedDate ?? item.Echo.DateCreated
                    : item.Echo.DateCreated)
                .ToList();

            ViewBag.FeedItems = sortedFeedItems;
            ViewBag.CurrentUser = currentUserId;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Type = TempData["type"];
            }

            ViewBag.Title = "Main Feed";
            SetAccessRights();
            return View();
        }

        // [HttpGet] care se executa implicit
        [Authorize(Roles = "Admin")]
        public IActionResult AdminInterface()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Type = TempData["type"];
            }

            ViewBag.Title = "Admin Interface";
            SetAccessRights();
            return View();
        }

        // [HttpGet] care se executa implicit
        [Authorize(Roles = "Admin")]
        public IActionResult IndexAdmin()
        {
            var echoes = db.Echoes
                            .Include(ech => ech.User)
                                .ThenInclude(u => u!.ApplicationUser)
                            .Include(ech => ech.AmpParent)
                                .ThenInclude(ech => ech!.User)
                                    .ThenInclude(u => u!.ApplicationUser)
                            .Include(ech => ech.Interactions)
                            .Include(ech => ech.Flock)
                            .OrderByDescending(ech => ech.DateCreated);

            ViewBag.Echoes = echoes;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Type = TempData["type"];
            }

            ViewBag.Title = "List of Echoes";
            SetAccessRights();
            return View();
        }

        // [HttpGet] care se executa implicit
        [Authorize(Roles = "User,Admin")]
        public IActionResult Amplifiers(int id)
        {
            var deletedUserId = db.Users
                .Where(u => u.UserName == "deleted")
                .Select(u => u.Id)
                .FirstOrDefault() ?? string.Empty;

            var echoes = db.Echoes
                            .Where(ech => !ech.IsRemoved && ech.UserId != deletedUserId && ech.AmpParentId == id) // Filtreaza echo-urile sterse
                            .Include(ech => ech.User)
                                .ThenInclude(u => u!.ApplicationUser)
                            .Include(ech => ech.AmpParent)
                                .ThenInclude(ech => ech!.User)
                                    .ThenInclude(u => u!.ApplicationUser)
                            .Include(ech => ech.Interactions)
                            .Include(ech => ech.Flock)
                            .OrderByDescending(ech => ech.DateCreated);

            ViewBag.Echoes = echoes;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Type = TempData["type"];
            }

            ViewBag.Title = "Amplifiers";
            SetAccessRights();
            return View();
        }

        // [HttpGet] care se executa implicit
        [Authorize(Roles = "User,Admin")]
        public IActionResult Bookmarks()
        {
            var deletedUserId = db.Users
                .Where(u => u.UserName == "deleted")
                .Select(u => u.Id)
                .FirstOrDefault() ?? string.Empty;

            var bookmarkEchoes = db.Interactions
                                    .Where(i => i.Bookmarked && i.UserId == _userManager.GetUserId(User) && !i.Echo!.IsRemoved && i.Echo.UserId != deletedUserId)
                                    .OrderByDescending(i => i.BookmarkedDate)
                                    .Include(i => i.Echo!.User)
                                        .ThenInclude(u => u!.ApplicationUser)
                                    .Include(i => i.Echo!.AmpParent)
                                        .ThenInclude(ech => ech!.User)
                                            .ThenInclude(u => u!.ApplicationUser)
                                    .Include(i => i.Echo!.Interactions)
                                    .Include(i => i.Echo!.Flock)
                                    .Select(i => i.Echo)
                                    .ToList();
                                    // useful! - .AsNoTracking() - lista / rezultatul in sine devine readonly si nu poate fi modificat de db.SaveChanges();
                                    
            ViewBag.Title = "Bookmarks";
            ViewBag.Echoes = bookmarkEchoes;
            SetAccessRights();
            return View();
        }

        // [HttpGet] se executa implicit
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Echo? echo = db.Echoes
                            .Include(ech => ech.Interactions)
                            .Include(ech => ech.Flock)
                            .Include(ech => ech.Comments!)
                                .ThenInclude(comm => comm.User)
                                    .ThenInclude(u => u!.ApplicationUser)
                            .Include(ech => ech.AmpParent)
                                .ThenInclude(ech => ech!.User)
                                    .ThenInclude(u => u!.ApplicationUser)
                            .Include(ech => ech.Comments!)
                                .ThenInclude(comm => comm.Interactions)
                            .Include(ech => ech.User)
                                .ThenInclude(u => u!.ApplicationUser)
                            .Where(ech => ech.Id == id)
                            .FirstOrDefault();

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            // get post parents
            List<Echo> parents = [];
            Echo? curr = echo;
            while (curr is not null && curr.CommParentId != null)
            {
                Echo? parent = db.Echoes
                            .Include(ech => ech.Interactions)
                            .Include(ech => ech.Flock)
                            .Include(ech => ech.User)
                                .ThenInclude(u => u!.ApplicationUser)
                            .Where(ech => ech.Id == curr.CommParentId)
                            .FirstOrDefault();

                if (parent != null)
                {
                    parents.Add(parent);
                }
                curr = parent;
            }

            parents.Reverse();
            ViewBag.Parents = parents;

            LoadCommentsRecursively(echo);

            // Set current user ID for view comparison
            ViewBag.CurrentUser = _userManager.GetUserId(User);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Type = TempData["type"];
            }

            ViewBag.Title = "View Echo";
            SetAccessRights();
            return View(echo);
        }


        // [HttpGet] se executa implicit
        [Authorize(Roles = "User, Admin")]
        public IActionResult New(int? id, string? type)
        {
            Echo echo = new();
            if (id != null && type != null)
            {
                Echo? parentEcho = db.Echoes.Include(e => e.User).ThenInclude(u => u!.ApplicationUser).Where(e => e.Id == id).FirstOrDefault();
                if (parentEcho == null)
                {
                    return RedirectToAction("Index");
                }

                if (type.Equals("comment"))
                {
                    echo.CommParentId = id;
                }
                else if (type.Equals("amplifier"))
                {
                    echo.AmpParentId = id;
                }

                echo.FlockId = parentEcho.FlockId;
                ViewBag.Flock = db.Flocks.Find(parentEcho.FlockId);
                ViewBag.Parent = parentEcho;
            }
            else
            {
                // Load all flocks for dropdown
                var flocks = db.Flocks
                    .Where(f => f.FlockStatus == "active")
                    .OrderBy(f => f.Name)
                    .ToList();

                ViewBag.Flocks = new SelectList(flocks, "Id", "Name");
            }

            ViewBag.Title = "New Echo";
            SetAccessRights();
            return View(echo);
        }

        // POST: Procesează datele trimise de utilizator
        [HttpPost]
        [Authorize(Roles = "User ,Admin")]
        public async Task<IActionResult> New(Echo echo, IFormFile? att1, IFormFile? att2)
        {
            echo.DateCreated = DateTime.Now;

            // get user for FK - now directly uses ApplicationUserId (string GUID)
            echo.UserId = _userManager.GetUserId(User) ?? string.Empty;

            // Validate file extensions before saving
            if (att1 != null && att1.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(att1.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Att1", "File #1 must be an image (jpg, jpeg, png, webp, gif) or a video (mp4, mov).");
                    SetAccessRights();
                    return View(echo);
                }
            }

            if (att2 != null && att2.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(att2.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Att2", "File #2 must be an image (jpg, jpeg, png, webp, gif) or a video (mp4, mov).");
                    SetAccessRights();
                    return View(echo);
                }
            }

            // Explicitly ensure Id is 0 for new entities to avoid identity column errors
            echo.Id = 0;

            if (TryValidateModel(echo))
            {
                db.Echoes.Add(echo);
                await db.SaveChangesAsync();

                // Now save files using the echo ID
                if (att1 != null && att1.Length > 0)
                {
                    var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Images", echo.Id.ToString());
                    Directory.CreateDirectory(directoryPath); // Create directory if it doesn't exist

                    var storagePath = Path.Combine(directoryPath, att1.FileName);
                    var databaseFileName = "/Resources/Alex/Images/" + echo.Id + "/" + att1.FileName;

                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await att1.CopyToAsync(fileStream);
                    }

                    echo.Att1 = databaseFileName;
                }

                if (att2 != null && att2.Length > 0)
                {
                    var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Images", echo.Id.ToString());
                    Directory.CreateDirectory(directoryPath); // Create directory if it doesn't exist

                    var storagePath = Path.Combine(directoryPath, att2.FileName);
                    var databaseFileName = "/Resources/Alex/Images/" + echo.Id + "/" + att2.FileName;

                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await att2.CopyToAsync(fileStream);
                    }

                    echo.Att2 = databaseFileName;
                }

                // Update echo with file paths if files were uploaded
                if (att1 != null || att2 != null)
                {
                    await db.SaveChangesAsync();
                }

                if (echo.CommParentId != null)
                {
                    TempData["message"] = "Comment was added succesfully!";
                    TempData["type"] = "alert-success";

                    Echo? parentEcho = db.Echoes.Where(e => e.Id == echo.CommParentId).FirstOrDefault();
                    if (parentEcho != null)
                    {
                        parentEcho.CommentsCount++;
                        await db.SaveChangesAsync();
                    }

                    return RedirectToAction("Show", new { id = echo.CommParentId });
                }
                else if (echo.AmpParentId != null)
                {
                    TempData["message"] = "Amplifier was added succesfully!";
                    TempData["type"] = "alert-success";

                    Echo? parentEcho = db.Echoes.Where(e => e.Id == echo.AmpParentId).FirstOrDefault();
                    if (parentEcho != null)
                    {
                        parentEcho.AmplifierCount++;
                        await db.SaveChangesAsync();
                    }

                    return RedirectToAction("Show", new { id = echo.Id });
                }
                else
                {
                    TempData["message"] = "Echo was sent succesfully!";
                    TempData["type"] = "alert-success";
                    return RedirectToAction("Index");
                }
                    
            }

            SetAccessRights();
            return View(echo);
        }

        // [HttpGet] se executa implicit
        [Authorize(Roles = "User, Admin")]
        public IActionResult Edit(int id)
        {
            Echo? echo = db.Echoes.Where(art => art.Id == id).FirstOrDefault();

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (echo.IsRemoved)
            {
                TempData["message"] = "Cannot edit a deleted echo!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (echo.UserId == _userManager.GetUserId(User))
            {
                ViewBag.Title = "Modify Echo";
                SetAccessRights();
                return View(echo);
            }
            else
            {
                TempData["message"] = "You are not authorized to modify this echo.";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult Edit(int id, Echo reqEcho, IFormFile? att1, IFormFile? att2, bool RemoveAtt1 = false, bool RemoveAtt2 = false)
        {
            Echo? echo = db.Echoes.Find(id);

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (echo.IsRemoved)
            {
                TempData["message"] = "Cannot edit a deleted echo!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (echo.UserId != _userManager.GetUserId(User) ||  !User.IsInRole("Admin"))
            {
                TempData["message"] = "You are not authorized to modify this echo.";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            echo.Content = reqEcho.Content;

            // Active after pressing delete and no new files
            if (RemoveAtt1 && !string.IsNullOrEmpty(echo.Att1))
            {
                DeletePhysicalFile(echo.Att1);
                echo.Att1 = null;
            }

            if (RemoveAtt2 && !string.IsNullOrEmpty(echo.Att2))
            {
                DeletePhysicalFile(echo.Att2);
                echo.Att2 = null;
            }

            if (att1 != null && att1.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(att1.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Att1", "File #1 must be an image (jpg, jpeg, png, webp, gif) or a video (mp4, mov).");
                    SetAccessRights();
                    return View(echo);
                }
            }

            if (att2 != null && att2.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(att2.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Att2", "File #2 must be an image (jpg, jpeg, png, webp, gif) or a video (mp4, mov).");
                    SetAccessRights();
                    return View(echo);
                }
            }


            if (att1 != null && att1.Length > 0)
            {
                var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Images", echo.Id.ToString());
                Directory.CreateDirectory(directoryPath);

                var storagePath = Path.Combine(directoryPath, att1.FileName);
                var databaseFileName = "/Resources/Alex/Images/" + echo.Id + "/" + att1.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    att1.CopyTo(fileStream);
                }

                echo.Att1 = databaseFileName;
            }
 

            if (att2 != null && att2.Length > 0)
            {
                var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Alex", "Images", echo.Id.ToString());
                Directory.CreateDirectory(directoryPath);

                var storagePath = Path.Combine(directoryPath, att2.FileName);
                var databaseFileName = "/Resources/Alex/Images/" + echo.Id + "/" + att2.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    att2.CopyTo(fileStream);
                }

                echo.Att2 = databaseFileName;
            }

            if (TryValidateModel(echo))
            {
                echo.DateEdited = DateTime.Now;
                db.SaveChanges();

                TempData["message"] = "Echo was modified succesfully!";
                TempData["type"] = "alert-info";
                return RedirectToAction("Show", new { id = echo.Id });
            }
            else
            {
                SetAccessRights();
                return View(echo);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public ActionResult Delete(int id, string? type)
        {
            Echo? echo = db.Echoes.Find(id);

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
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
                TempData["message"] = "Deleted user not found!";
                TempData["type"] = "alert-danger";
                if (type == null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("IndexAdmin");
                }
            }

            if (echo.UserId == deletedUser.Id)
            {
                TempData["message"] = "Echo was already deleted!";
                TempData["type"] = "alert-warning";
                if (type == null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("IndexAdmin");
                }
            }

            if (echo.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                // Atribuie echo-ul și comentariile la utilizatorul deleted
                AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);

                try
                {
                    db.SaveChanges();
                    TempData["message"] = "Echo was deleted!";
                    TempData["type"] = "alert-info";
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
                    TempData["message"] = "Echo could not be deleted...";
                    TempData["type"] = "alert-danger";
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
                TempData["message"] = "You do not have the necessary permissions to delete this echo.";
                TempData["type"] = "alert-warning";
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
        public ActionResult DeleteAdmin(int id)
        {
            Echo? echo = db.Echoes.Find(id);

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("IndexAdmin");
            }

            if (User.IsInRole("Admin"))
            {
                RemoveParentEcho(echo);

                try
                {
                    db.SaveChanges();
                    TempData["message"] = "Echo was deleted!";
                    TempData["type"] = "alert-info";
                    return RedirectToAction("IndexAdmin");
                }
                catch (DbUpdateException)
                {
                    TempData["message"] = "Echo could not be deleted...";
                    TempData["type"] = "alert-danger";
                    return RedirectToAction("IndexAdmin");
                }
            }
            else
            {
                TempData["message"] = "You do not have the necessary permissions to delete this echo.";
                TempData["type"] = "alert-warning";
                return RedirectToAction("IndexAdmin");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult MarkAllDeletedAdmin()
        {
            if (User.IsInRole("Admin"))
            {
                var echoes = db.Echoes.Where(ech => !ech.IsRemoved).OrderByDescending(ech => ech.DateCreated).ToList();

                if (echoes is null)
                {
                    TempData["message"] = "There are no echoes not marked as deleted!";
                    TempData["type"] = "alert-warning";
                    return RedirectToAction("IndexAdmin");
                }

                var deletedUser = db.Users.Where(u => u.UserName == "deleted").FirstOrDefault();

                if (deletedUser == null)
                {
                    TempData["message"] = "Deleted user not found!";
                    TempData["type"] = "alert-danger";
                    return RedirectToAction("IndexAdmin");
                }

                foreach (var echo in echoes)
                {
                    AssignEchoAndChildrenToDeletedUser(echo, deletedUser.Id);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                        TempData["message"] = "Echo could not be deleted...";
                        TempData["type"] = "alert-danger";
                        return RedirectToAction("IndexAdmin");
                    }
                }
            }
            else
            {
                TempData["message"] = "You do not have the necessary permissions to delete all echoes.";
                TempData["type"] = "alert-warning";
                return RedirectToAction("IndexAdmin");
            }

            TempData["message"] = "All non-\"deleted\" Echoes were marked as deleted!";
            TempData["type"] = "alert-info";
            return RedirectToAction("IndexAdmin");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAllAdmin()
        {
            if (User.IsInRole("Admin"))
            {
                var echoes = db.Echoes.OrderByDescending(ech => ech.DateCreated).ToList();

                if (echoes is null)
                {
                    TempData["message"] = "There are no Echoes in the database!";
                    TempData["type"] = "alert-warning";
                    return RedirectToAction("IndexAdmin");
                }
            
                foreach (var echo in echoes)
                {
                    RemoveParentEcho(echo);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                        TempData["message"] = "Echo could not be deleted...";
                        TempData["type"] = "alert-danger";
                        return RedirectToAction("IndexAdmin");
                    }
                }
            }
            else
            {
                TempData["message"] = "You do not have the necessary permissions to delete all echoes.";
                TempData["type"] = "alert-warning";
                return RedirectToAction("IndexAdmin");
            }

            TempData["message"] = "All Echoes have been permanently deleted!";
            TempData["type"] = "alert-info";
            return RedirectToAction("IndexAdmin");
        }


        // other methods
        // Conditii de afisare pt butoanele de afisare / stergere din views
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

        // Incarcarea comentariilor pt arborele de relatii al postarii curente
        private void LoadCommentsRecursively(Echo echo)
        {
            if (echo.Comments is null)
            {
                echo.Comments = new List<Echo>();
            }

            var comments = db.Echoes
                            .Include(ech => ech.Interactions)
                            .Include(ech => ech.Flock)
                            .Include(ech => ech.User)
                            .Where(ech => ech.CommParentId == echo.Id)
                            .ToList();
            echo.Comments = comments;

            foreach (var comment in comments)
            {
                LoadCommentsRecursively(comment);
            }
        }

        // highest ancestor
        private int GetOriginalEchoId(Echo echo)
        {
            // Obține ID-ul utilizatorului deleted pentru a-l exclude
            var deletedUserId = db.Users
                .Where(u => u.UserName == "deleted")
                .Select(u => u.Id)
                .FirstOrDefault() ?? string.Empty;

            Echo? current = echo;
            while (current.CommParentId != null)
            {
                current = db.Echoes
                    .Where(e => e.Id == current.CommParentId && e.UserId != deletedUserId) // Trebuie sa filtreze echo-urile sterse? - && !e.IsRemoved 
                    .FirstOrDefault();
                if (current == null) 
                    break;
            }
            return current?.Id ?? echo.Id;
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

        private void RemoveParentEcho(Echo echo)
        {
            // Set CommParent to null for children
            var commChildren = db.Echoes.Where(ech => ech.CommParentId == echo.Id).ToList();

            foreach (var comm in commChildren)
            {
                comm.CommParentId = null;
                comm.CommParent = null;
            }

            // Set AmpParent to null for children
            var ampChildren = db.Echoes.Where(ech => ech.AmpParentId == echo.Id).ToList();

            foreach (var amp in ampChildren)
            {
                amp.AmpParentId = null;
                amp.AmpParent = null;
            }

            // Remove all Interaction columns that use this echo
            var interactions = db.Interactions.Where(i => i.EchoId == echo.Id).ToList();

            foreach (var inter in interactions)
            {
                db.Interactions.Remove(inter);
            }

            if (echo.Att1 != null)
            {
                DeletePhysicalFile(echo.Att1);
            }
            if (echo.Att2 != null)
            {
                DeletePhysicalFile(echo.Att2);
            }

            string? folder = null;
            if (echo.Att1 != null)
            {
                var lastSlashIndex = echo.Att1.LastIndexOf('/');
                if (lastSlashIndex > 0)
                {
                    folder = echo.Att1.Substring(0, lastSlashIndex);
                }
            }
            else if (echo.Att2 != null)
            {
                var lastSlashIndex = echo.Att2.LastIndexOf('/');
                if (lastSlashIndex > 0)
                {
                    folder = echo.Att2.Substring(0, lastSlashIndex);
                }
            }

            if (folder != null)
            {
                var folderPath = Path.Combine(_env.WebRootPath, folder.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (Directory.Exists(folderPath))
                {
                    try
                    {
                        Directory.Delete(folderPath, recursive: true);
                    }
                    catch (DirectoryNotFoundException)
                    {
                      
                    }
                    catch (IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not delete directory {folderPath}: {ex.Message}");
                    }
                }
            }

            db.Echoes.Remove(echo);
        }

        // Stergerea fisierului din wwwroot
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
