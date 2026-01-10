using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAtwitter_DAW_2526.Models
{
    public class UserProfile
    {
        [Key]
        public string Id { get; set; } = string.Empty; // ApplicationUserId

        [Required(ErrorMessage = "The user must have a display name;")]
        [MaxLength(50, ErrorMessage = "The display name must have a maximum of 50 characters;")]
        public string DisplayName { get; set; } = string.Empty;
        [MaxLength(200, ErrorMessage = "The description must have a maximum of 200 characters;")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "The user must have a profile picture (default if not selected);")]
        public string PfpLink { get; set; } = "/Resources/Images/user_default_pfp.jpg";
        [Required(ErrorMessage = "The user must have a banner (default if not selected);")]
        public string BannerLink { get; set; } = "/Resources/Images/banner_default.jpg";
 
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public string? Pronouns { get; set; }
        public string AccountStatus { get; set; } = "active";

        public virtual ApplicationUser? ApplicationUser { get; set; }
        public virtual ICollection<FlockUser>? FlockUsers { get; set; }
        public virtual ICollection<Bookmark>? Bookmarks { get; set; }
        public virtual ICollection<Interaction>? Interactions { get; set; }

        [InverseProperty(nameof(Relation.Sender))]
        public virtual ICollection<Relation> SentRelations { get; set; } = new List<Relation>();

        [InverseProperty(nameof(Relation.Receiver))]
        public virtual ICollection<Relation> ReceivedRelations { get; set; } = new List<Relation>();

        [InverseProperty(nameof(FollowRequest.SenderUser))]
        public virtual ICollection<FollowRequest> SentFollowRequests { get; set; } = new List<FollowRequest>();

        [InverseProperty(nameof(FollowRequest.ReceiverUser))]
        public virtual ICollection<FollowRequest> ReceivedFollowRequests { get; set; } = new List<FollowRequest>();
    }
}
