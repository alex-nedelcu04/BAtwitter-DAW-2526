namespace BAtwitter_DAW_2526.Models
{
    public class Bookmark
    {
        public int UserId { get; set; }
        public int? EchoId { get; set; }

        public DateTime AddDate { get; set; } = DateTime.Now;

        public virtual ApplicationUser? User { get; set; }
        public virtual Echo? Echo { get; set; }

    }
}
