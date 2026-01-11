using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult Search(string? query)
        {
            var search = "";
            if (Convert.ToString(HttpContext.Request.Query["query"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["query"]).Trim();

                var deletedUser = db.Users
                    .Where(u => u.UserName == "deleted")
                    .FirstOrDefault();

                var currentUserId = _userManager.GetUserId(User);

                var nonCommentEchoes = db.Echoes
                                        .Include(ech => ech.User)
                                            .ThenInclude(u => u!.SentRelations)
                                        .Include(ech => ech.User)
                                            .ThenInclude(u => u!.ReceivedRelations)
                                        .Include(ech => ech.User)
                                            .ThenInclude(u => u!.ApplicationUser)
                                        .Include(ech => ech.AmpParent)
                                            .ThenInclude(ech => ech!.User)
                                                .ThenInclude(u => u!.ApplicationUser)
                                        .Include(ech => ech.Interactions!)
                                            .ThenInclude(i => i.User)
                                                .ThenInclude(u => u!.ApplicationUser)
                                        .Include(ech => ech.Flock)
                                        .ToList()
                                        .Where(ech => ((ech.Content != null && ech.Content.Contains(search, StringComparison.OrdinalIgnoreCase)) || (ech.Flock != null && ech.Flock.Name.Contains(search))) &&
                                                        !ech.IsRemoved && ech.UserId != deletedUser!.Id && CanViewEcho(ech) && ech.CommParentId == null)
                                        .OrderByDescending(ech => ech.DateCreated);

                ViewBag.NonComments = nonCommentEchoes;

                var commentEchoes = db.Echoes
                                        .Include(ech => ech.User)
                                            .ThenInclude(u => u!.SentRelations)
                                        .Include(ech => ech.User)
                                            .ThenInclude(u => u!.ReceivedRelations)
                                        .Include(ech => ech.User)
                                            .ThenInclude(u => u!.ApplicationUser)
                                        .Include(ech => ech.CommParent)
                                            .ThenInclude(prnt => prnt!.User)
                                                .ThenInclude(u => u!.ApplicationUser)
                                        .Include(ech => ech.AmpParent)
                                            .ThenInclude(ech => ech!.User)
                                                .ThenInclude(u => u!.ApplicationUser)
                                        .Include(ech => ech.Interactions!)
                                            .ThenInclude(i => i.User)
                                                .ThenInclude(u => u!.ApplicationUser)
                                        .Include(ech => ech.Flock)
                                        .ToList()
                                        .Where(ech => ((ech.Content != null && ech.Content.Contains(search, StringComparison.OrdinalIgnoreCase)) || (ech.Flock != null && ech.Flock.Name.Contains(search))) &&
                                                        !ech.IsRemoved && ech.UserId != deletedUser!.Id && CanViewEcho(ech) && ech.CommParentId != null)
                                        .OrderByDescending(ech => ech.DateCreated);

                ViewBag.Comments = commentEchoes;

                var flocks = db.Flocks
                                .Include(f => f.Admin!)
                                    .ThenInclude(a => a.ApplicationUser)
                                .Include(f => f.Echos!)
                                    .ThenInclude(e => e.User!)
                                        .ThenInclude(u => u.ApplicationUser)
                                .ToList()
                                .Where(fl => !fl.FlockStatus.Equals("deleted") && fl.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                                .OrderByDescending(ech => ech.DateCreated);

                Dictionary<int, Echo?> echoes = [];
                foreach (var fl in flocks)
                {
                    Echo? ech = db.Echoes
                                    .Include(e => e.Flock)
                                    .Where(e => e.FlockId == fl.Id && !e.IsRemoved && e.UserId != deletedUser!.Id && e.CommParentId == null)
                                    .OrderByDescending(e => e.DateCreated)
                                    .FirstOrDefault();

                    echoes.TryAdd(fl.Id, ech);
                }

                ViewBag.Flocks = flocks;
                ViewBag.EchoMap = echoes;

                if (_userManager.GetUserId(User) != null)
                {
                    var userProfiles = db.UserProfiles
                                        .Include(up => up.ApplicationUser)
                                        .Include(up => up.SentRelations)
                                        .Include(up => up.ReceivedRelations)
                                        .ToList()
                                        .Where(up => !up.AccountStatus.Equals("deleted") &&
                                                  ((up.ApplicationUser!.UserName!.Contains(search, StringComparison.OrdinalIgnoreCase)) || (up.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase))) &&
                                                  !up.SentRelations.Any(rel => rel.ReceiverId == currentUserId && rel.Type == -1))
                                        .OrderByDescending(up => up.JoinDate);
                    ViewBag.Users = userProfiles;
                }
                else
                {
                    var userProfiles = db.UserProfiles
                                        .Include(up => up.ApplicationUser)
                                        .ToList()
                                        .Where(up => !up.AccountStatus.Equals("deleted") && 
                                                  ((up.ApplicationUser!.UserName!.Contains(search, StringComparison.OrdinalIgnoreCase)) || (up.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase))) &&
                                                  !up.SentRelations.Any(rel => rel.ReceiverId == currentUserId && rel.Type == -1))
                                        .OrderByDescending(up => up.JoinDate);
                    ViewBag.Users = userProfiles;
                }
            }

            ViewBag.SearchString = query;
            ViewBag.Title = "Search Results";
            SetAccessRights();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult New_Like(int EchoId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["message"] = "Not logged in!";
                TempData["type"] = "alert-danger";
                return Json(new { success = false, error = "Not logged in!" });
            }

            Echo? echo = db.Echoes
                .Include(e => e.User)
                .FirstOrDefault(e => e.Id == EchoId);
            
            if (echo == null)
            {
                TempData["message"] = "Not logged in!";
                TempData["type"] = "alert-danger";
                return Json(new { success = false, error = "Echo does not exist!" });
            }

            // Check if users are blocked
            var isBlocked = db.Relations
                .Any(r => (r.SenderId == userId && r.ReceiverId == echo.UserId && r.Type == -1) ||
                          (r.SenderId == echo.UserId && r.ReceiverId == userId && r.Type == -1));

            if (isBlocked)
            {
                return Json(new { success = false, error = "You cannot interact with this user's content." });
            }

            Interaction? inter = db.Interactions.Find(userId, EchoId);

            if (inter == null)
            {
                // Create new interaction and like it
                inter = new Interaction
                {
                    UserId = userId,
                    EchoId = EchoId,
                    Liked = true,
                    LikedDate = DateTime.Now
                };
                db.Interactions.Add(inter);
                echo.LikesCount++;
                
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    // Race condition: interaction was created by another request
                    // Reload from database and toggle
                    db.Entry(inter).State = EntityState.Detached;
                    db.Entry(echo).State = EntityState.Detached;
                    inter = db.Interactions.Find(userId, EchoId);
                    echo = db.Echoes.Find(EchoId);
                    if (inter != null && echo != null)
                    {
                        inter.Liked = !inter.Liked;
                        inter.LikedDate = inter.Liked ? DateTime.Now : null;
                        echo.LikesCount = inter.Liked ? echo.LikesCount + 1 : echo.LikesCount - 1;
                        db.SaveChanges();
                    }
                    else
                    {
                        return Json(new { success = false, error = "Error updating interaction" });
                    }
                }
            }
            else
            {
                // Toggle like
                if (inter.Liked)
                {
                    inter.Liked = false;
                    inter.LikedDate = null;
                    echo.LikesCount--;
                }
                else
                {
                    inter.Liked = true;
                    inter.LikedDate = DateTime.Now;
                    echo.LikesCount++;
                }
                db.SaveChanges();
            }

            // Return JSON response for AJAX
            return Json(new { 
                success = true, 
                isLiked = inter.Liked, 
                likesCount = echo.LikesCount 
            });
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult New_Rebound(int EchoId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["message"] = "Not logged in!";
                TempData["type"] = "alert-danger";
                return Json(new { success = false, error = "Not logged in!" });
            }

            Echo? echo = db.Echoes
                .Include(e => e.User)
                .FirstOrDefault(e => e.Id == EchoId);
            
            if (echo == null)
            {
                TempData["message"] = "Not logged in!";
                TempData["type"] = "alert-danger";
                return Json(new { success = false, error = "Echo does not exist!" });
            }

            // Check if users are blocked (bidirectional)
            var isBlocked = db.Relations
                .Any(r => (r.SenderId == userId && r.ReceiverId == echo.UserId && r.Type == -1) ||
                          (r.SenderId == echo.UserId && r.ReceiverId == userId && r.Type == -1));

            if (isBlocked)
            {
                return Json(new { 
                    success = false, 
                    error = "You cannot interact with this user's content.",
                    isRebounded = false,
                    reboundCount = echo.ReboundCount
                });
            }

            Interaction? inter = db.Interactions.Find(userId, EchoId);
            bool canRebound = echo.UserId == _userManager.GetUserId(User) || echo.User!.AccountStatus.Equals("active") && !db.Relations.Any(r => (r.ReceiverId == userId && r.SenderId == echo.UserId) && r.Type == -1);

            if (canRebound)
            {
                if (inter == null)
                {
                    // Create new interaction and rebound it
                    inter = new Interaction
                    {
                        UserId = userId,
                        EchoId = EchoId,
                        Rebounded = true,
                        ReboundedDate = DateTime.Now
                    };
                    db.Interactions.Add(inter);
                    echo.ReboundCount++;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                        // Race condition: interaction was created by another request
                        // Reload from database and toggle
                        db.Entry(inter).State = EntityState.Detached;
                        db.Entry(echo).State = EntityState.Detached;
                        inter = db.Interactions.Find(userId, EchoId);
                        echo = db.Echoes.Find(EchoId);
                        if (inter != null && echo != null)
                        {
                            inter.Rebounded = !inter.Rebounded;
                            inter.ReboundedDate = inter.Rebounded ? DateTime.Now : null;
                            echo.ReboundCount = inter.Rebounded ? echo.ReboundCount + 1 : echo.ReboundCount - 1;
                            db.SaveChanges();
                        }
                        else
                        {
                            return Json(new { success = false, error = "Error updating interaction" });
                        }
                    }
                }
                else
                {
                    // Toggle rebound
                    if (inter.Rebounded)
                    {
                        inter.Rebounded = false;
                        inter.ReboundedDate = null;
                        echo.ReboundCount--;
                    }
                    else
                    {
                        inter.Rebounded = true;
                        inter.ReboundedDate = DateTime.Now;
                        echo.ReboundCount++;
                    }
                    db.SaveChanges();
                }

                // Return JSON response for AJAX
                return Json(new
                {
                    success = true,
                    isRebounded = inter.Rebounded,
                    reboundCount = echo.ReboundCount
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    error = "Cannot rebound Private Account",
                    isRebounded = false,
                    reboundCount = echo.ReboundCount
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult New_Bookmark(int EchoId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["message"] = "Not logged in!";
                TempData["type"] = "alert-danger";
                return Json(new { success = false, error = "Not logged in!" });
            }

            Echo? echo = db.Echoes
                .Include(e => e.User)
                .FirstOrDefault(e => e.Id == EchoId);
            
            if (echo == null)
            {
                return Json(new { success = false, error = "Echo does not exist!" });
            }

            // Check if users are blocked
            var isBlocked = db.Relations
                .Any(r => (r.SenderId == userId && r.ReceiverId == echo.UserId && r.Type == -1) ||
                          (r.SenderId == echo.UserId && r.ReceiverId == userId && r.Type == -1));

            if (isBlocked)
            {
                return Json(new { success = false, error = "You cannot interact with this user's content." });
            }

            Interaction? inter = db.Interactions.Find(userId, EchoId);

            if (inter == null)
            {
                inter = new Interaction
                {
                    UserId = userId,
                    EchoId = EchoId,
                    Bookmarked = true,
                    BookmarkedDate = DateTime.Now
                };
                db.Interactions.Add(inter);
                echo.BookmarksCount++;
                
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    // Race condition: interaction was created by another request
                    // Reload from database and toggle
                    db.Entry(inter).State = EntityState.Detached;
                    db.Entry(echo).State = EntityState.Detached;
                    inter = db.Interactions.Find(userId, EchoId);
                    echo = db.Echoes.Find(EchoId);
                    if (inter != null && echo != null)
                    {
                        inter.Bookmarked = !inter.Bookmarked;
                        inter.BookmarkedDate = inter.Bookmarked ? DateTime.Now : null;
                        echo.BookmarksCount = inter.Bookmarked ? echo.BookmarksCount + 1 : echo.BookmarksCount - 1;
                        db.SaveChanges();
                    }
                    else
                    {
                        return Json(new { success = false, error = "Error updating interaction" });
                    }
                }
            }
            else
            {
                if (inter.Bookmarked)
                {
                    inter.Bookmarked = false;
                    inter.BookmarkedDate = null;
                    echo.BookmarksCount--;
                }
                else
                {
                    inter.Bookmarked = true;
                    inter.BookmarkedDate = DateTime.Now;
                    echo.BookmarksCount++;
                }
                db.SaveChanges();
            }

            // Return JSON response for AJAX
            return Json(new { 
                success = true, 
                isBookmarked = inter.Bookmarked, 
                bookmarksCount = echo.BookmarksCount 
            });
        }



        // Other methods
        private bool CanViewEcho(Echo echo)
        {
            var currentUserId = _userManager.GetUserId(User);
 
            
            // If echo user is blocked by current user or has blocked current user, don't show
            if (!string.IsNullOrEmpty(currentUserId) && 
                (echo.User!.SentRelations.Any(rel => rel.ReceiverId == currentUserId && rel.Type == -1)))
            {
                return false;
            }

            return User.IsInRole("Admin") || echo.User!.AccountStatus.Equals("active")
               || (echo.User!.AccountStatus.Equals("private") && echo.User!.ReceivedRelations.Any(rel => rel.SenderId == currentUserId && rel.Type == 1))
               || echo.UserId == currentUserId;
        }

        private bool CanViewPrivs(UserProfile usr)
        {
            var currentUserId = _userManager.GetUserId(User);
            
            if (User.IsInRole("Admin"))
                return true;
            
            if (currentUserId == usr.ApplicationUser!.Id)
                return true;
            
            bool isBlocked = usr.SentRelations.Any(rel => rel.ReceiverId == currentUserId && rel.Type == -1);
            
            if (isBlocked)
                return false;
            
            if (usr.AccountStatus.Equals("active"))
                return true;
            
            
            return false;
        }

        private void SetAccessRights()
        {
            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.IsAdmin = User.IsInRole("Admin");
        }
    }
}
