using AspNetCoreGeneratedDocument;
using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        // Se afiseaza lista tuturor articolelor impreuna cu categoria 
        // din care fac parte
        // Pentru fiecare articol se afiseaza si userul care a postat articolul
        // [HttpGet] care se executa implicit
        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Index()
        {
            var echoes = db.Echoes
                            .Include(ech => ech.User)
                                .ThenInclude(u => u.ApplicationUser)
                            .Include(ech => ech.Interactions)
                            .Include(ech => ech.Flock)
                            .OrderByDescending(ech => ech.DateCreated);

            ViewBag.Echoes = echoes;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Type = TempData["type"];
            }

            return View();
        }

        // Se afiseaza un singur articol in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui articol
        // Se afiseaza si userul care a postat articolul
        // [HttpGet] se executa implicit
        //[Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            Echo? echo = db.Echoes
                            .Include(ech => ech.Interactions)
                            .Include(ech => ech.Flock)
                            .Include(ech => ech.Comments)
                                .ThenInclude(comm => comm.User)
                                    .ThenInclude(u => u.ApplicationUser)
                            .Include(ech => ech.Comments)
                                .ThenInclude(comm => comm.Interactions)
                            .Include(ech => ech.User)
                                .ThenInclude(u => u.ApplicationUser)
                            .Where(ech => ech.Id == id)
                            .FirstOrDefault();

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            // get post parents

            /*
            ViewBag.Parents = new List<Echo>();
            Echo? curr = echo;
            while (curr.CommParentId != null)
            {
                Echo? parent = db.Echoes
                            .Include(ech => ech.Interactions)
                            .Include(ech => ech.Flock)
                            .Include(ech => ech.User)
                            .Where(ech => ech.Id == curr.CommParentId)
                            .FirstOrDefault();

                if (parent != null)
                {
                    ViewBag.Parents.Add(parent);
                }
                curr = parent;
            }
            */

            LoadCommentsRecursively(echo);

            // Set current user ID for view comparison
            ViewBag.CurrentUser = _userManager.GetUserId(User);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Type = TempData["type"];
            }

            return View(echo);
        }

        // Se afiseaza formularul in care se vor completa datele unui articol
        // impreuna cu selectarea categoriei din care face parte
        // Doar userii cu rol de Editor / Admin pot adauga noi articole
        // [HttpGet] se executa implicit
        //[Authorize(Roles = "Editor,Admin")]

        public IActionResult New()
        {
            Echo echo = new();

            // Load all flocks for dropdown
            var flocks = db.Flocks
                .Where(f => f.FlockStatus == "active")
                .OrderBy(f => f.Name)
                .ToList();

            ViewBag.Flocks = new SelectList(flocks, "Id", "Name");

            return View(echo);
        }

        // POST: Procesează datele trimise de utilizator
        // Doar userii cu rolul Editor / Admin pot adauga noi articole
        //[Authorize(Roles = "Editor,Admin")]

        [HttpPost]
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
                    return View(echo);
                }
            }

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

                TempData["message"] = "Echo was sent succesfully!";
                TempData["type"] = "alert-success";

                return RedirectToAction("Index");
            }

            return View(echo);
        }

        /*
        // Adaugarea unui comentariu asociat unui articol in baza de date
        [HttpPost]
        public IActionResult New(Comment comm)
        {
            comm.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }

            return Redirect("/Articles/Show/" + comm.ArticleId);
        }
        */


        // Se editeaza un articol existent in baza de date impreuna cu categoria din care face parte
        // Categoria se selecteaza dintr-un dropdown
        // [HttpGet] se executa implicit
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        // Doar userii cu rolul Editor / Admin pot edita articolele
        // Admin  -> oricare articol din BD
        // Editor -> doar articolele scrise de ei insisi
        //[Authorize(Roles = "Editor,Admin")]
        
        public IActionResult Edit(int id)
        {
            Echo? echo = db.Echoes.Where(art => art.Id == id).FirstOrDefault();

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (echo.UserId == _userManager.GetUserId(User))
            {
                return View(echo);
            }
            else
            {
                TempData["message"] = "You are not authorized to modify this echo.";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }
        }

        // Se adauga articolul modificat in baza de date
        // Se verifica rolul userului pentru a vedea daca poate edita
        [HttpPost]
        //[Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id, Echo reqEcho, IFormFile? att1, IFormFile? att2, bool RemoveAtt1 = false, bool RemoveAtt2 = false)
        {
            Echo? echo = db.Echoes.Find(id);

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (echo.UserId != _userManager.GetUserId(User))
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
                return View(echo);
            }
        }


        // Se sterge un articol din baza de date
        // Editorii sau Adminii pot sterge articole
        // Editor -> articolele lor
        // Admin  -> oricare articol din BD
        [HttpPost]
        //[Authorize(Roles = "Editor,Admin")]
        
        public ActionResult Delete(int id)
        {
            Echo? echo = db.Echoes.Find(id);

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (echo.UserId == _userManager.GetUserId(User))
            {
                db.Echoes.Remove(echo);

                try
                {
                    db.SaveChanges();
                    TempData["message"] = "Echo was deleted!";
                    TempData["type"] = "alert-info";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    TempData["message"] = "Echo could not be deleted...";
                    TempData["type"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["message"] = "You do not have the necessary permissions to modify this article.";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Show(int? id, int? EchoId, string? Content)
        {
            int echoId = id ?? EchoId ?? 0;

            if (echoId == 0)
            {
                TempData["message"] = "Invalid Echo ID!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            Echo? parentEcho = db.Echoes.Find(echoId);

            if (parentEcho is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(Content))
            {
                TempData["message"] = "Comment content cannot be empty!";
                TempData["type"] = "alert-warning";
             

                int originalPostId = GetOriginalEchoId(parentEcho);
                return RedirectToAction("Show", new { id = originalPostId });
            }

            // Create a new Echo as a comment
            Echo comment = new Echo
            {
                Content = Content,
                CommParentId = echoId,
                UserId = _userManager.GetUserId(User) ?? string.Empty,
                DateCreated = DateTime.Now
            };

            db.Echoes.Add(comment);
            db.SaveChanges();

            TempData["message"] = "Comment added successfully!";
            TempData["type"] = "alert-success";

            // Redirect to the original post
            int originalEchoId = GetOriginalEchoId(parentEcho);
            return RedirectToAction("Show", originalEchoId);

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
            Echo? current = echo;
            while (current.CommParentId != null)
            {
                current = db.Echoes.Find(current.CommParentId);
                if (current == null) 
                    break;
            }
            return current?.Id ?? echo.Id;
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
