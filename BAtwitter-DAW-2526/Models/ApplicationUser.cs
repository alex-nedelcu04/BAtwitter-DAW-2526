using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAtwitter_DAW_2526.Models
{
    [Index(nameof(UserName), IsUnique = true)]
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "The user must have a diplay name \uD83D")]
        public string DisplayName { get; set; } = string.Empty;
        [MaxLength(150,ErrorMessage = "The description must have a maximum of 150 characters \uD83D")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "The user will have a profile picture \uD83D (default if not selected)")]
        public string PfpLink { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public string? Pronouns { get; set; }
        public string AccountStatus { get; set; } = "active";
        public virtual ICollection<FlockUser>? FlockUsers { get; set; }
        public virtual ICollection<Bookmark>? Bookmarks { get; set; }


    }
}
