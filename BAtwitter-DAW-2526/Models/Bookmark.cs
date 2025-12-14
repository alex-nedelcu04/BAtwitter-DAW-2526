using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAtwitter_DAW_2526.Models
{

    public class Bookmark
    {
        public string UserId { get; set; } = string.Empty;
        public int EchoId { get; set; }

        public DateTime AddDate { get; set; } = DateTime.Now;

        public virtual UserProfile? User { get; set; }
        public virtual Echo? Echo { get; set; }

    }
}
