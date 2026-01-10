using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAtwitter_DAW_2526.Models
{
    public class Echo
    {
        [Key]
        public int Id { get; set; }

        public int? FlockId { get; set; }
        public string UserId { get; set; } = string.Empty;

        public int? CommParentId { get; set; } // comment
        public int? AmpParentId { get; set; } // amplifier

        [MaxLength(500, ErrorMessage = "The content of the Echo cannot exceed 500 characters;")]
        public string? Content { get; set; }
        public string? Att1 { get; set; }
        public string? Att2 { get; set; }

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int ReboundCount { get; set; }
        public int AmplifierCount { get; set; }
        public int BookmarksCount { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateEdited { get; set; } = null;
        public bool IsRemoved { get; set; } = false;

        public virtual Flock? Flock { get; set; }
        public virtual UserProfile? User { get; set; }

        public virtual Echo? CommParent { get; set; }
        public virtual Echo? AmpParent { get; set; }

        public virtual ICollection<Echo>? Comments { get; set; }
        public virtual ICollection<Echo>? Amplifiers { get; set; }
        public virtual ICollection<Bookmark>? Bookmarks {  get; set; }
        public virtual ICollection<Interaction>? Interactions { get; set; }

    }
}
