namespace BAtwitter_DAW_2526.Models
{
    public class Interaction
    {
        public string UserId { get; set; } = string.Empty;
        public int EchoId { get; set; }

        public bool Liked { get; set; } = false;
        public bool Bookmarked { get; set; } = false;
        public bool Rebounded { get; set; } = false;
        public DateTime? LikedDate { get; set; }
        public DateTime? ReboundedDate { get; set; }
        public DateTime? BookmarkedDate { get; set; }

        public virtual Echo? Echo { get; set; }
        public virtual UserProfile? User { get; set; }

    }
}
