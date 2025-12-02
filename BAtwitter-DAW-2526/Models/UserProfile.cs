using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAtwitter_DAW_2526.Models
{
    public class UserProfile
    {

        [Key]
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "The user must have a diplay name \uD83D")]
        public string DisplayName { get; set; } = string.Empty;
        [MaxLength(150, ErrorMessage = "The description must have a maximum of 150 characters \uD83D")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "The user will have a profile picture \uD83D (default if not selected)")]
        public string PfpLink { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public string? Pronouns { get; set; }
        public string AccountStatus { get; set; } = "active";

        public virtual ApplicationUser ApplicationUser { get; set; } = new ApplicationUser();
        public virtual ICollection<FlockUser>? FlockUsers { get; set; }
        public virtual ICollection<Bookmark>? Bookmarks { get; set; }
        public virtual ICollection<Interaction>? Interactions { get; set; }

        [InverseProperty(nameof(Relation.Sender))]
        public virtual ICollection<Relation> SentRelations { get; set; } = new List<Relation>();

        [InverseProperty(nameof(Relation.Receiver))]
        public virtual ICollection<Relation> ReceivedRelations { get; set; } = new List<Relation>();
    }
}
