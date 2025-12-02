using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAtwitter_DAW_2526.Models
{

    public class Relation
    {
  
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }

        public virtual UserProfile? Sender { get; set; }

        public virtual UserProfile? Receiver { get; set; }

        public DateTime relationDate { get; set; } = DateTime.Now;
        public int type = 1; // followed = 1, blocked = -1


    }
}
