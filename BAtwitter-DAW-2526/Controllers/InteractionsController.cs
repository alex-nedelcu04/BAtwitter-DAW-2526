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

            Echo? echo = db.Echoes.Find(EchoId);
            if (echo == null)
            {
                TempData["message"] = "Not logged in!";
                TempData["type"] = "alert-danger";
                return Json(new { success = false, error = "Echo does not exist!" });
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

            Echo? echo = db.Echoes.Find(EchoId);
            if (echo == null)
            {
                TempData["message"] = "Not logged in!";
                TempData["type"] = "alert-danger";
                return Json(new { success = false, error = "Echo does not exist!" });
            }

            Interaction? inter = db.Interactions.Find(userId, EchoId);

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
            return Json(new { 
                success = true, 
                isRebounded = inter.Rebounded, 
                reboundCount = echo.ReboundCount 
            });
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

            Echo? echo = db.Echoes.Find(EchoId);
            if (echo == null)
            {
                return Json(new { success = false, error = "Echo does not exist!" });
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
    }
}
