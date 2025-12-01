using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace BAtwitter_DAW_2526.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Echo> Echoes { get; set; }
        public DbSet<Flock> Flocks { get; set; }
        public DbSet<FlockUser> FlockUsers { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<Relation> Relations { get; set; }
    }
}
