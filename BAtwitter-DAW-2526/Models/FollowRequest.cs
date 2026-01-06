using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAtwitter_DAW_2526.Models
{
    public class FollowRequest
    {
        [Key]
        public int Id { get; set; }

        public string SenderUserId { get; set; } = string.Empty;
        public string ReceiverUserId { get; set; } = string.Empty;
        public int? ReceiverFlockId { get; set; }

        public virtual UserProfile? SenderUser { get; set; }
        public virtual UserProfile? ReceiverUser { get; set; }
        public virtual Flock? ReceiverFlock { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;
    }
}
