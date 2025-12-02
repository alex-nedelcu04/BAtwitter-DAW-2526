using Microsoft.EntityFrameworkCore;

namespace BAtwitter_DAW_2526.Models
{
    [PrimaryKey(nameof(UserId), nameof(EchoId))]
    public class Bookmark
    {
        public int UserId { get; set; }
        public int EchoId { get; set; }

        public DateTime AddDate { get; set; } = DateTime.Now;

        public virtual UserProfile? User { get; set; }
        public virtual Echo? Echo { get; set; }

    }
}
