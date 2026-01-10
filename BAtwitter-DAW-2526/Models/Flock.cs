using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BAtwitter_DAW_2526.Models
{
    public class Flock
    {
        [Key]
        public int Id { get; set; }
        public string AdminId { get; set; } = string.Empty;

        public virtual UserProfile? Admin { get; set; }

        [Required]
        [MaxLength(30, ErrorMessage = "The name must have a maximum of 30 characters;")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "The description must have a maximum of 150 characters;")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "The flock must have a profile picture (default if not selected);")]
        public string PfpLink { get; set; } = "/Resources/Images/flock_default_pfp.jpg";
        [Required(ErrorMessage = "The flock must have a banner (default if not selected);")]
        public string BannerLink { get; set; } = "/Resources/Images/banner_default.jpg";

        public DateTime DateCreated { get; set; } = DateTime.Now;
        public string FlockStatus { get; set; } = "active";

        public virtual ICollection<FlockUser>? FlockUsers { get; set; }

        public virtual ICollection<Echo>? Echos { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.InverseProperty(nameof(FollowRequest.ReceiverFlock))]
        public virtual ICollection<FollowRequest> FollowRequests { get; set; } = new List<FollowRequest>();


    }
}
