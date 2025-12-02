namespace BAtwitter_DAW_2526.Models
{
    public class FlockUser
    {
        public int FlockId { get; set; }
        public int UserId { get; set; }

        public virtual Flock? Flock { get; set; }
        public virtual UserProfile? User { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;

        public string Role { get; set; } = "member"; // member / moderator / admin


    }
}
