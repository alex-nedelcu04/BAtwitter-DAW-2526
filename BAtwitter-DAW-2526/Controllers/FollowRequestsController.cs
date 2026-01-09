using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BAtwitter_DAW_2526.Controllers
{
    public class FollowRequestsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> usrm)
        {
            db = context;
            _userManager = usrm;
        }

        // TO DO !!!!!!
        //          DONE - UNFOLLOW (similar cu FollowDirect pretty much, doar ca nu creezi relatia, o stergi)
        //          DONE - Kind reminder pt mine: Retweet si quote nu se poate la postarile unui cont privat la care nu ai follow
        //          DONE - O sa trebuiasca si un view pt followeri si following
        //          DONE - De sters alertele de la inceputul paginii, sa ramana doar acolo jos
        //          DONE - Am uitat sa fac schimbarea aia cu numele paginii sa fei scris langa search
        //          DONE - inca se vad postarile unui user privat in profilul lui
        //          DONE - adminul paginii nu are buton de follow, apar tot alea de mark as deleted si delete permanently
        //          DONE - Delete Flock Admin => Adminul site-ului devine adminul flockului
        //          DONE - ADD SEARCH METHOD
        //          DONE - Make profiles with wrong relations not able to view followers / follows as well
        //          DONE - Add click event to follows/followers display name / username
        //          DONE - No login into "deleted" accounts
        //          DONE - When UnfollowUser, if from Followers / Following redirect to own page instead
        //   IN PROGRESS - Edit Flock => Adminul poate asigna alt admin prin introducerea usernameului cu un searchbar (does not work yet at all basically)
        // Make regular user delete not assign posts to deleted user, only admin delete (?????????????)
        // Block Users (maybe Flocks as well care da block la toti userii din flock?)
        // ADD AI FUNCTIONALITY
        // SEED DATA FINAL SI RESETAREA FISIERELOR DI BD-URILOR PT CLEAN TESTING
        // Alte chestii de frontend, cum ar fi butoane de rebound / amplify mai subtile daca e cont privat, ### LoginPartial modificat - DONE ###,
        //                                     SCOS / MODIFICAT HOME CONTROLLER, poate sa ne mai uitam peste taburile din sidebar,
        //                                     vedem ce facem cu paginile de Identity, formul de new/edit echo sa arate bine + add a new comment button

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult SendFollowRequest(string receiverUserId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to send a follow request.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            var receiverUser = db.UserProfiles
                .Include(u => u.ApplicationUser)
                .FirstOrDefault(u => u.Id == receiverUserId);

            if (receiverUser == null)
            {
                TempData["followrequest-message"] = "User not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "UserProfiles");
            }

            if (receiverUserId == currentUserId)
            {
                TempData["followrequest-message"] = "You cannot send a follow request to yourself.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "UserProfiles", new { username = receiverUser.ApplicationUser?.UserName });
            }

            var existingRelation = db.Relations
                .FirstOrDefault(r => r.SenderId == currentUserId && r.ReceiverId == receiverUserId);
            
            if (existingRelation != null)
            {
                TempData["followrequest-message"] = "You are already following this user.";
                TempData["followrequest-type"] = "alert-info";
                return RedirectToAction("Show", "UserProfiles", new { username = receiverUser.ApplicationUser?.UserName });
            }

            var blockedRelation = db.Relations
                .FirstOrDefault(r => (r.SenderId == currentUserId && r.ReceiverId == receiverUserId && r.Type == -1) ||
                                     (r.SenderId == receiverUserId && r.ReceiverId == currentUserId && r.Type == -1));
            
            if (blockedRelation != null)
            {
                TempData["followrequest-message"] = "You cannot send a follow request to this user.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "UserProfiles", new { username = receiverUser.ApplicationUser?.UserName });
            }

            var existingRequest = db.FollowRequests
                .FirstOrDefault(fr => fr.SenderUserId == currentUserId && 
                                     fr.ReceiverUserId == receiverUserId && 
                                     fr.ReceiverFlockId == null);
            
            if (existingRequest != null)
            {
                TempData["followrequest-message"] = "You already have a pending follow request for this user.";
                TempData["followrequest-type"] = "alert-info";
                return RedirectToAction("Show", "UserProfiles", new { username = receiverUser.ApplicationUser?.UserName });
            }

            // Create follow request
            var followRequest = new FollowRequest
            {
                SenderUserId = currentUserId,
                ReceiverUserId = receiverUserId,
                ReceiverFlockId = null,
                RequestDate = DateTime.Now
            };

            db.FollowRequests.Add(followRequest);
            db.SaveChanges();

            TempData["followrequest-message"] = "Follow request sent successfully!";
            TempData["followrequest-type"] = "alert-success";
            return RedirectToAction("Show", "UserProfiles", new { username = receiverUser.ApplicationUser?.UserName });
        }


        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult FollowDirect(string receiverUserId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to follow a user.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            var receiverUser = db.UserProfiles
                .Include(u => u.ApplicationUser)
                .FirstOrDefault(u => u.Id == receiverUserId);

            if (receiverUser == null)
            {
                TempData["followrequest-message"] = "User not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "UserProfiles");
            }

            if (receiverUserId == currentUserId)
            {
                TempData["followrequest-message"] = "You cannot follow yourself.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "UserProfiles", new { username = receiverUser.ApplicationUser?.UserName });
            }

            if (receiverUser.AccountStatus == "private")
            {
                return RedirectToAction("SendFollowRequest", new { receiverUserId = receiverUserId });
            }

            var existingRelation = db.Relations
                .FirstOrDefault(r => r.SenderId == currentUserId && r.ReceiverId == receiverUserId && r.Type == 1);
            
            if (existingRelation != null)
            {
                TempData["followrequest-message"] = "You are already following this user.";
                TempData["followrequest-type"] = "alert-info";
                return RedirectToAction("Show", "UserProfiles", new { username = receiverUser.ApplicationUser?.UserName });
            }

            var blockedRelation = db.Relations
                .FirstOrDefault(r => (r.SenderId == currentUserId && r.ReceiverId == receiverUserId && r.Type == -1) ||
                                     (r.SenderId == receiverUserId && r.ReceiverId == currentUserId && r.Type == -1));
            
            if (blockedRelation != null)
            {
                TempData["followrequest-message"] = "You cannot follow this user.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "UserProfiles", new { username = receiverUser.ApplicationUser?.UserName });
            }

            // Create the follow relation directly
            var relation = new Relation
            {
                SenderId = currentUserId,
                ReceiverId = receiverUserId,
                relationDate = DateTime.Now,
                Type = 1 // followed
            };

            db.Relations.Add(relation);
            db.SaveChanges();

            TempData["followrequest-message"] = "You are now following this user!";
            TempData["followrequest-type"] = "alert-success";
            return RedirectToAction("Show", "UserProfiles", new { username = receiverUser.ApplicationUser?.UserName });
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult UnfollowUser(string receiverUserId, string? type)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to unfollow a user.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            var senderUser = db.UserProfiles.Select(u => u.ApplicationUser).FirstOrDefault(u => u!.Id == currentUserId);
            var receiverUser = db.UserProfiles
                .Include(u => u.ApplicationUser)
                .FirstOrDefault(u => u.Id == receiverUserId);

            if (receiverUser == null)
            {
                TempData["followrequest-message"] = "User not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "UserProfiles");
            }

            if (receiverUserId == currentUserId)
            {
                TempData["followrequest-message"] = "You cannot unfollow yourself.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "UserProfiles", new { username = (type != null) ? senderUser!.UserName : receiverUser.ApplicationUser?.UserName });
            }

            /*
            if (receiverUser.AccountStatus == "private")
            {
                return RedirectToAction("SendFollowRequest", new { receiverUserId = receiverUserId });
            }*/

            var existingRelation = db.Relations
                .FirstOrDefault(r => r.SenderId == currentUserId && r.ReceiverId == receiverUserId && r.Type == 1);

            if (existingRelation == null)
            {
                TempData["followrequest-message"] = "You are not following this user.";
                TempData["followrequest-type"] = "alert-info";
                return RedirectToAction("Show", "UserProfiles", new { username = (type != null) ? senderUser!.UserName : receiverUser.ApplicationUser?.UserName });
            }

            var blockedRelation = db.Relations
                .FirstOrDefault(r => (r.SenderId == currentUserId && r.ReceiverId == receiverUserId && r.Type == -1) ||
                                     (r.SenderId == receiverUserId && r.ReceiverId == currentUserId && r.Type == -1));

            if (blockedRelation != null)
            {
                TempData["followrequest-message"] = "You cannot unfollow this user.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "UserProfiles", new { username = (type != null) ? senderUser!.UserName : receiverUser.ApplicationUser?.UserName });
            }

            
            db.Relations.Remove(existingRelation);
            db.SaveChanges();

            TempData["followrequest-message"] = "You have unfollowed this user!";
            TempData["followrequest-type"] = "alert-info";
            return RedirectToAction("Show", "UserProfiles", new { username = (type != null) ? senderUser!.UserName : receiverUser.ApplicationUser?.UserName });
        }

        /// <summary>
        /// Accept a follow request (for user-to-user follows)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult AcceptFollowRequest(string senderUserId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to accept a follow request.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            // Find the follow request
            var followRequest = db.FollowRequests
                .FirstOrDefault(fr => fr.SenderUserId == senderUserId && 
                                     fr.ReceiverUserId == currentUserId && 
                                     fr.ReceiverFlockId == null);

            if (followRequest == null)
            {
                TempData["followrequest-message"] = "Follow request not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("PendingUserRequests");
            }

            
            var existingRelation = db.Relations
                .FirstOrDefault(r => r.SenderId == senderUserId && r.ReceiverId == currentUserId);
            
            if (existingRelation == null)
            {
                // Create the follow relation
                var relation = new Relation
                {
                    SenderId = senderUserId,
                    ReceiverId = currentUserId,
                    relationDate = DateTime.Now,
                    Type = 1 // followed
                };
                db.Relations.Add(relation);
            }


            db.FollowRequests.Remove(followRequest);
            db.SaveChanges();

            TempData["followrequest-message"] = "Follow request accepted!";
            TempData["followrequest-type"] = "alert-success";
            return RedirectToAction("PendingUserRequests");
        }


        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult RejectFollowRequest(string senderUserId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to reject a follow request.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

     
            var followRequest = db.FollowRequests
                .FirstOrDefault(fr => fr.SenderUserId == senderUserId && 
                                     fr.ReceiverUserId == currentUserId && 
                                     fr.ReceiverFlockId == null);

            if (followRequest == null)
            {
                TempData["followrequest-message"] = "Follow request not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("PendingUserRequests");
            }
            db.FollowRequests.Remove(followRequest);
            db.SaveChanges();

            TempData["followrequest-message"] = "Follow request rejected.";
            TempData["followrequest-type"] = "alert-info";
            return RedirectToAction("PendingUserRequests");
        }

     
        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult CancelFollowRequest(string receiverUserId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to cancel a follow request.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            var followRequest = db.FollowRequests
                .FirstOrDefault(fr => fr.SenderUserId == currentUserId && 
                                     fr.ReceiverUserId == receiverUserId && 
                                     fr.ReceiverFlockId == null);

            if (followRequest == null)
            {
                TempData["followrequest-message"] = "Follow request not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("PendingUserRequests");
            }

            // Remove the follow request
            db.FollowRequests.Remove(followRequest);
            db.SaveChanges();

            TempData["followrequest-message"] = "Follow request cancelled.";
            TempData["followrequest-type"] = "alert-info";
            return RedirectToAction("PendingUserRequests");
        }

      
        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult SendFlockJoinRequest(int flockId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to send a join request.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            var flock = db.Flocks
                .Include(f => f.Admin)
                .FirstOrDefault(f => f.Id == flockId);

            if (flock == null)
            {
                TempData["followrequest-message"] = "Flock not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Flocks");
            }

            if (flock.FlockStatus != "active")
            {
                TempData["followrequest-message"] = "This flock is not accepting new members.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "Flocks", new { id = flockId });
            }

            var existingMember = db.FlockUsers
                .FirstOrDefault(fu => fu.FlockId == flockId && fu.UserId == currentUserId);
            
            if (existingMember != null)
            {
                TempData["followrequest-message"] = "You are already a member of this flock.";
                TempData["followrequest-type"] = "alert-info";
                return RedirectToAction("Show", "Flocks", new { id = flockId });
            }

            if (flock.AdminId == currentUserId)
            {
                TempData["followrequest-message"] = "You are the admin of this flock.";
                TempData["followrequest-type"] = "alert-info";
                return RedirectToAction("Show", "Flocks", new { id = flockId });
            }

            var existingRequest = db.FollowRequests
                .FirstOrDefault(fr => fr.SenderUserId == currentUserId && 
                                     fr.ReceiverFlockId == flockId);
            
            if (existingRequest != null)
            {
                TempData["followrequest-message"] = "You already have a pending join request for this flock.";
                TempData["followrequest-type"] = "alert-info";
                return RedirectToAction("Show", "Flocks", new { id = flockId });
            }

            // Create join request
            var joinRequest = new FollowRequest
            {
                SenderUserId = currentUserId,
                ReceiverUserId = flock.AdminId,
                ReceiverFlockId = flockId,
                RequestDate = DateTime.Now
            };

            db.FollowRequests.Add(joinRequest);
            db.SaveChanges();

            TempData["followrequest-message"] = "Join request sent successfully!";
            TempData["followrequest-type"] = "alert-success";
            return RedirectToAction("Show", "Flocks", new { id = flockId });
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult UnfollowFlock(int flockId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to unfollow a Flock.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            // Find the flock
            var flock = db.Flocks.Find(flockId);
            if (flock == null)
            {
                TempData["followrequest-message"] = "Flock not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Flocks");
            }

            // Check if user is already a member
            var existingMember = db.FlockUsers
                .FirstOrDefault(fu => fu.FlockId == flockId && fu.UserId == userId);

            if (existingMember == null)
            {
                TempData["followrequest-message"] = "You must be a member to unfollow a flock.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Flocks");
            }

            db.FlockUsers.Remove(existingMember);
            db.SaveChanges();

            TempData["followrequest-message"] = "Unfollowed flock!";
            TempData["followrequest-type"] = "alert-success";
            return RedirectToAction("Show", "Flocks", new { id = flockId });
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult AcceptFlockJoinRequest(int flockId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to accept a join request.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            // Find the flock
            var flock = db.Flocks.Find(flockId);
            if (flock == null)
            {
                TempData["followrequest-message"] = "Flock not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Flocks");
            }

            if (flock.AdminId != currentUserId && !User.IsInRole("Admin"))
            {
                TempData["followrequest-message"] = "You are not authorized to accept join requests for this flock.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "Flocks", new { id = flockId });
            }

            var joinRequest = db.FollowRequests
                .FirstOrDefault(fr => fr.SenderUserId == userId && 
                                     fr.ReceiverFlockId == flockId);

            if (joinRequest == null)
            {
                TempData["followrequest-message"] = "Join request not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("PendingFlockRequests", new { flockId = flockId });
            }

            // Check if user is already a member
            var existingMember = db.FlockUsers
                .FirstOrDefault(fu => fu.FlockId == flockId && fu.UserId == userId);
            
            if (existingMember == null)
            {
                // Create FlockUser entry
                var flockUser = new FlockUser
                {
                    FlockId = flockId,
                    UserId = userId,
                    JoinDate = DateTime.Now,
                    Role = "member"
                };
                db.FlockUsers.Add(flockUser);
            }

            db.FollowRequests.Remove(joinRequest);
            db.SaveChanges();

            TempData["followrequest-message"] = "Join request accepted!";
            TempData["followrequest-type"] = "alert-success";
            return RedirectToAction("PendingFlockRequests", new { flockId = flockId });
        }


        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult RejectFlockJoinRequest(int flockId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to reject a join request.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            // Find the flock
            var flock = db.Flocks.Find(flockId);
            if (flock == null)
            {
                TempData["followrequest-message"] = "Flock not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Flocks");
            }
            if (flock.AdminId != currentUserId && !User.IsInRole("Admin"))
            {
                TempData["followrequest-message"] = "You are not authorized to reject join requests for this flock.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "Flocks", new { id = flockId });
            }

            var joinRequest = db.FollowRequests
                .FirstOrDefault(fr => fr.SenderUserId == userId && 
                                     fr.ReceiverFlockId == flockId);

            if (joinRequest == null)
            {
                TempData["followrequest-message"] = "Join request not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("PendingFlockRequests", new { flockId = flockId });
            }

            // Remove the join request
            db.FollowRequests.Remove(joinRequest);
            db.SaveChanges();

            TempData["followrequest-message"] = "Join request rejected.";
            TempData["followrequest-type"] = "alert-info";
            return RedirectToAction("PendingFlockRequests", new { flockId = flockId });
        }


        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult CancelFlockJoinRequest(int flockId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["followrequest-message"] = "You must be logged in to cancel a join request.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            // Find the join request
            var joinRequest = db.FollowRequests
                .FirstOrDefault(fr => fr.SenderUserId == currentUserId && 
                                     fr.ReceiverFlockId == flockId);

            if (joinRequest == null)
            {
                TempData["followrequest-message"] = "Join request not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "Flocks", new { id = flockId });
            }


            db.FollowRequests.Remove(joinRequest);
            db.SaveChanges();

            TempData["followrequest-message"] = "Join request cancelled.";
            TempData["followrequest-type"] = "alert-info";
            return RedirectToAction("Show", "Flocks", new { id = flockId });
        }


        [Authorize(Roles = "User, Admin")]
        public IActionResult PendingUserRequests()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", "Identity");
            }

            // Get sent requests (both user-to-user and flock)
            var sentRequests = db.FollowRequests
                .Include(fr => fr.ReceiverUser!)
                    .ThenInclude(u => u.ApplicationUser)
                .Include(fr => fr.ReceiverFlock)
                .Where(fr => fr.SenderUserId == currentUserId)
                .OrderByDescending(fr => fr.RequestDate)
                .ToList();

            // Get received requests (user-to-user only, since flock requests go to admin)
            var receivedRequests = db.FollowRequests
                .Include(fr => fr.SenderUser!)
                    .ThenInclude(u => u.ApplicationUser)
                .Where(fr => fr.ReceiverUserId == currentUserId && fr.ReceiverFlockId == null)
                .OrderByDescending(fr => fr.RequestDate)
                .ToList();

            ViewBag.SentRequests = sentRequests;
            ViewBag.ReceivedRequests = receivedRequests;
            ViewBag.CurrentUser = currentUserId;

            if (TempData.ContainsKey("followrequest-message"))
            {
                ViewBag.Message = TempData["followrequest-message"];
                ViewBag.Type = TempData["followrequest-type"];
            }

            ViewBag.Title = "Follow Requests";
            return View();
        }


        [Authorize(Roles = "User, Admin")]
        public IActionResult PendingFlockRequests(int flockId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", "Identity");
            }

            var flock = db.Flocks
                .Include(f => f.Admin)
                .FirstOrDefault(f => f.Id == flockId);

            if (flock == null)
            {
                TempData["followrequest-message"] = "Flock not found.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Index", "Flocks");
            }

            if (flock.AdminId != currentUserId && !User.IsInRole("Admin"))
            {
                TempData["followrequest-message"] = "You are not authorized to view join requests for this flock.";
                TempData["followrequest-type"] = "alert-warning";
                return RedirectToAction("Show", "Flocks", new { id = flockId });
            }

            var pendingRequests = db.FollowRequests
                .Include(fr => fr.SenderUser!)
                    .ThenInclude(u => u.ApplicationUser)
                .Where(fr => fr.ReceiverFlockId == flockId)
                .OrderByDescending(fr => fr.RequestDate)
                .ToList();

            ViewBag.PendingRequests = pendingRequests;
            ViewBag.CurrentUser = currentUserId;

            if (TempData.ContainsKey("followrequest-message"))
            {
                ViewBag.Message = TempData["followrequest-message"];
                ViewBag.Type = TempData["followrequest-type"];
            }

            ViewBag.Title = "Flock Join Requests";
            return View(flock);
        }

    }
}
