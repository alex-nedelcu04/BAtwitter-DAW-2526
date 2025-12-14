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
                            .Include(ech => ech.Comments!)
                                .ThenInclude(comm => comm.User)
                            .Include(ech => ech.User)
                            .Where(ech => ech.Id == id)
                            .FirstOrDefault();

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            // get post parents
            ViewBag.Parents = new List<Echo>();
            Echo? curr = echo;
            while (curr != null && curr.CommParentId != null)
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

            //SetAccessRights();

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

            return View(echo);
        }

        // POST: Procesează datele trimise de utilizator
        // Doar userii cu rolul Editor / Admin pot adauga noi articole
        //[Authorize(Roles = "Editor,Admin")]

        [HttpPost]
        public async Task<IActionResult> New(Echo echo, IFormFile? att1, IFormFile? att2)
        {
            echo.DateCreated = DateTime.Now;

          

            ModelState.Remove(nameof(echo.User.ApplicationUser));
            ModelState.Remove(nameof(echo.User.DisplayName));
            ModelState.Remove(nameof(echo.User.PfpLink));
            
            if (att1 != null && att1.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(att1.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Image", "File #1 must be an image (jpg, jpeg, png, webp, gif) or a video (mp4, mov).");
                    return View(echo);
                }

                var storagePath = Path.Combine(_env.WebRootPath, "Resources/Images", att1.FileName);
                var databaseFileName = "/Resources/Images/" + att1.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await att1.CopyToAsync(fileStream);
                }

                ModelState.Remove(nameof(echo.Att2));
                echo.Att2 = databaseFileName;
            }

            if (att2 != null && att2.Length > 0)
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(att2.FileName).ToLower();
                if (!extensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Image", "File #2 must be an image (jpg, jpeg, png, webp, gif) or a video (mp4, mov).");
                    return View(echo);
                }

                var storagePath = Path.Combine(_env.WebRootPath, "Resources/Images", att2.FileName);
                var databaseFileName = "/Resources/Images/" + att2.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await att2.CopyToAsync(fileStream);
                }

                ModelState.Remove(nameof(echo.Att1));
                echo.Att1 = databaseFileName;
            }

            if (TryValidateModel(echo))
            {
                db.Echoes.Add(echo);
                await db.SaveChangesAsync();

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
            Echo? echo = db.Echoes.Where(ech => ech.Id == id).FirstOrDefault();

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (Convert.ToString(echo.UserId) == _userManager.GetUserId(User))
            {
                return View(echo);
            }
            else
            {
                TempData["message"] = "You do not have the necessary permissions to modify this article.";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }
        }

        // Se adauga articolul modificat in baza de date
        // Se verifica rolul userului pentru a vedea daca poate edita
        [HttpPost]
        //[Authorize(Roles = "Editor,Admin")]
        
        public IActionResult Edit(int id, Echo reqEcho)
        {
            Echo? echo = db.Echoes.Find(id);

            if (echo is null)
            {
                TempData["message"] = "Echo does not exist!";
                TempData["type"] = "alert-warning";
                return RedirectToAction("Index");
            }

            if (TryValidateModel(echo))
            {
                if (Convert.ToString(echo.UserId) == _userManager.GetUserId(User))
                {
                    echo.Content = reqEcho.Content;
                    echo.Att1 = reqEcho.Att1;
                    echo.Att2 = reqEcho.Att2;
                    echo.DateEdited = DateTime.Now;

                    db.SaveChanges();

                    TempData["message"] = "Echo was modified succesfully!";
                    TempData["type"] = "alert-info";
                    return RedirectToAction("Show", echo.Id);
                }
                else
                {
                    TempData["message"] = "You do not have the necessary permissions to modify this article.";
                    TempData["type"] = "alert-warning";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(reqEcho);
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

            if (Convert.ToString(echo.UserId) == _userManager.GetUserId(User))
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
    }
}
