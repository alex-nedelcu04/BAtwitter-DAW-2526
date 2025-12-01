using System.ComponentModel.DataAnnotations;

namespace BAtwitter_DAW_2526.Models
{
    public class Relation
    {
        [Key]
        public int? SenderId;
        [Key]
        public int? RecieverId;

        public virtual ApplicationUser? Sender { get; set; }
        public virtual ApplicationUser? Reciever { get; set; }

        public DateTime relationDate { get; set; } = DateTime.Now;
        public int type = 1; // followed = 1, blocked = -1


    }
}
