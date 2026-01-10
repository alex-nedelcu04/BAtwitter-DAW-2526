using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAtwitter_DAW_2526.Models
{

    public class Relation
    {
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;

        public virtual UserProfile? Sender { get; set; }
        public virtual UserProfile? Receiver { get; set; }

        public DateTime RelationDate { get; set; } = DateTime.Now;
        public int Type { get; set; } = 1; // followed = 1, blocked = -1
    }
}
