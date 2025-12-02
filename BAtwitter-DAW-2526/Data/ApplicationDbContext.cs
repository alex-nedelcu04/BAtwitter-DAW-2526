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
        public DbSet<UserProfile> UserProfiles { get; set; }
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

 
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(b => new { b.UserId, b.EchoId });

                entity.HasOne(b => b.User)
                      .WithMany(u => u.Bookmarks)
                      .HasForeignKey(b => b.UserId);

                entity.HasOne(b => b.Echo)
                      .WithMany(e => e.Bookmarks)
                      .HasForeignKey(b => b.EchoId);
            });

            modelBuilder.Entity<FlockUser>(entity =>
            {
                entity.HasKey(fu => new { fu.FlockId, fu.UserId });

                entity.HasOne(fu => fu.Flock)  
                      .WithMany(f => f.FlockUsers)
                      .HasForeignKey(fu => fu.FlockId);

                entity.HasOne(fu => fu.User)
                      .WithMany(u => u.FlockUsers)
                      .HasForeignKey(fu => fu.UserId);
            });

            modelBuilder.Entity<Interaction>(entity =>
            {
                entity.HasKey(i => new { i.UserId, i.EchoId });

                entity.HasOne(i => i.User)
                      .WithMany(u => u.Interactions)
                      .HasForeignKey(i => i.UserId);

                entity.HasOne(i => i.Echo)
                      .WithMany(e => e.Interactions)
                      .HasForeignKey(i => i.EchoId);
            });

            modelBuilder.Entity<Relation>(entity =>
            {
                entity.HasKey(r => new { r.SenderId, r.ReceiverId });

                entity.HasOne(r => r.Sender)
                      .WithMany(u => u.SentRelations)
                      .HasForeignKey(r => r.SenderId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.Receiver)
                      .WithMany(u => u.ReceivedRelations)
                      .HasForeignKey(r => r.ReceiverId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Echo>(entity =>
            {
                entity.HasOne(e => e.CommParent)
                      .WithMany(e => e.Comments)
                      .HasForeignKey(e => e.CommParentId);

                entity.HasOne(e => e.AmpParent)
                      .WithMany(e => e.Amplifiers)
                      .HasForeignKey(e => e.AmpParentId);
            });
        }
    }
}
