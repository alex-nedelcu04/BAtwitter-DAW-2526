// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;


namespace BAtwitter_DAW_2526.Areas.Identity.Pages.Account
{
    public partial class RegisterModel : PageModel
    {
        private static readonly string[] AllowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        private const string DefaultPfp = "/Resources/Images/user_default_pfp.jpg";
        private const string DefaultBanner = "/Resources/Images/banner_default.jpg";
        private const string TempUploadsWebPathPrefix = "/Resources/TempUploads/";

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _env = env;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            // Columns for equivalence with user profile
            [Required(ErrorMessage = "The user will have a tag \uD83D")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
            [Display(Name = "Username")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "The user must have a diplay name \uD83D")]
            public string DisplayName { get; set; } = string.Empty;
            [MaxLength(150, ErrorMessage = "The description must have a maximum of 150 characters \uD83D")]
            public string? Description { get; set; }

            public string? Pronouns { get; set; }

            [Required(ErrorMessage = "The user will have a profile picture \uD83D (default if not selected)")]
            public string PfpLink { get; set; } = "/Resources/Images/user_default_pfp.jpg";
            [Required(ErrorMessage = "The user will have a banner \uD83D (default if not selected)")]
            public string BannerLink { get; set; } = "/Resources/Images/banner_default.jpg";
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Ensure defaults are present so the view can render previews.
            Input ??= new InputModel();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            UserProfile userPf = new();
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // IMPORTANT: browser file inputs can't be repopulated after a postback for security reasons.
            // We keep the *preview* by temporarily persisting uploads under wwwroot and storing the URL in hidden fields.

            var pfpFile = Request.Form.Files["pfp"];
            var bannerFile = Request.Form.Files["banner"];

            if (!ModelState.IsValid)
            {
                await PreserveUploadsForRedisplayAsync(pfpFile, bannerFile);
                return Page();
            }

            var user = CreateUser();
            await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await PreserveUploadsForRedisplayAsync(pfpFile, bannerFile);
                return Page();
            }

            _logger.LogInformation("User created a new account with password.");
            await _userManager.AddToRoleAsync(user, "User");

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            // Creează UserProfile (start from whatever links we already have in the hidden fields)
            userPf = new UserProfile
            {
                Id = userId,
                DisplayName = Input.DisplayName,
                Description = Input.Description,
                Pronouns = Input.Pronouns,
                PfpLink = string.IsNullOrWhiteSpace(Input.PfpLink) ? DefaultPfp : Input.PfpLink,
                BannerLink = string.IsNullOrWhiteSpace(Input.BannerLink) ? DefaultBanner : Input.BannerLink,
                JoinDate = DateTime.Now,
                AccountStatus = "active"
            };

            userPf.PfpLink = await PersistFinalImageAsync(userId, pfpFile, Input.PfpLink, kind: "pfp", defaultLink: DefaultPfp);
            userPf.BannerLink = await PersistFinalImageAsync(userId, bannerFile, Input.BannerLink, kind: "banner", defaultLink: DefaultBanner);

            // Salvează UserProfile
            _context.UserProfiles.Add(userPf);
            await _context.SaveChangesAsync();

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }

        private async Task PreserveUploadsForRedisplayAsync(IFormFile? pfpFile, IFormFile? bannerFile)
        {
            Input ??= new InputModel();

            if (pfpFile != null && pfpFile.Length > 0)
            {
                var tmp = await SaveTempUploadAsync(pfpFile, "pfp");
                if (!string.IsNullOrEmpty(tmp))
                {
                    Input.PfpLink = tmp;
                }
            }
            else if (string.IsNullOrWhiteSpace(Input.PfpLink))
            {
                Input.PfpLink = DefaultPfp;
            }

            if (bannerFile != null && bannerFile.Length > 0)
            {
                var tmp = await SaveTempUploadAsync(bannerFile, "banner");
                if (!string.IsNullOrEmpty(tmp))
                {
                    Input.BannerLink = tmp;
                }
            }
            else if (string.IsNullOrWhiteSpace(Input.BannerLink))
            {
                Input.BannerLink = DefaultBanner;
            }
        }

        private async Task<string?> SaveTempUploadAsync(IFormFile file, string kind)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(fileExtension))
            {
                var field = kind == "pfp" ? "Input.PfpLink" : "Input.BannerLink";
                ModelState.AddModelError(field, "Accepted formats: jpg, jpeg, png, webp.");
                return null;
            }

            var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "TempUploads");
            Directory.CreateDirectory(directoryPath);

            var fileName = $"{Guid.NewGuid():N}_{kind}{fileExtension}";
            var storagePath = Path.Combine(directoryPath, fileName);

            using (var fileStream = new FileStream(storagePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return TempUploadsWebPathPrefix + fileName;
        }

        private async Task<string> PersistFinalImageAsync(
            string userId,
            IFormFile? uploadFile,
            string? existingLink,
            string kind,
            string defaultLink)
        {
            // 1) If the user uploaded a new file on this successful post, save it.
            if (uploadFile != null && uploadFile.Length > 0)
            {
                var fileExtension = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(fileExtension))
                {
                    return defaultLink;
                }

                var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Ioan", "Users", userId);
                Directory.CreateDirectory(directoryPath);

                // Keep original name, but strip any path pieces.
                var safeName = Path.GetFileName(uploadFile.FileName);
                var storagePath = Path.Combine(directoryPath, safeName);
                var databaseFileName = "/Resources/Ioan/Users/" + userId + "/" + safeName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await uploadFile.CopyToAsync(fileStream);
                }

                return databaseFileName;
            }

            // 2) Otherwise, if we already have a temp upload link from a previous failed post, move/copy it.
            if (!string.IsNullOrWhiteSpace(existingLink) && existingLink.StartsWith(TempUploadsWebPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var rel = existingLink.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var tempPath = Path.Combine(_env.WebRootPath, rel);

                if (System.IO.File.Exists(tempPath))
                {
                    var directoryPath = Path.Combine(_env.WebRootPath, "Resources", "Ioan", "Users", userId);
                    Directory.CreateDirectory(directoryPath);

                    var destName = Path.GetFileName(tempPath);
                    var destPath = Path.Combine(directoryPath, destName);

                    System.IO.File.Copy(tempPath, destPath, overwrite: true);
                    try { System.IO.File.Delete(tempPath); } catch { /* best effort cleanup */ }

                    return "/Resources/Ioan/Users/" + userId + "/" + destName;
                }
            }

            // 3) Fall back to default.
            return defaultLink;
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
