using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAtwitter_DAW_2526.Models
{
    [PrimaryKey(nameof(SenderId), nameof(ReceiverId))]
    public class Relation
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }

        [ForeignKey(nameof(SenderId))]
        [InverseProperty(nameof(UserProfile.SentRelations))]
        public virtual UserProfile? Sender { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        [InverseProperty(nameof(UserProfile.ReceivedRelations))]
        public virtual UserProfile? Receiver { get; set; }

        public DateTime relationDate { get; set; } = DateTime.Now;
        public int type = 1; // followed = 1, blocked = -1


    }
}
