using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BAtwitter_DAW_2526.Models
{
    public class Flock
    {
        [Key]
        public int Id { get; set; }
        public int AdminId { get; set; }

        public virtual UserProfile Admin { get; set; } = new UserProfile();

        public string Name { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "The description must have a maximum of 150 characters \uD83D")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "The flock will have a profile picture \uD83D (default if not selected)")]
        public string? PfpLink { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.Now;
        public string FlockStatus { get; set; } = "active";

        public virtual ICollection<FlockUser>? FlockUsers { get; set; }

        public virtual ICollection<Echo>? Echos { get; set; }


    }
}
