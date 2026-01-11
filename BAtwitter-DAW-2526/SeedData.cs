using BAtwitter_DAW_2526.Data;
using BAtwitter_DAW_2526.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BAtwitter_DAW_2526
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (!context.Roles.Any())
                {
                    // CREAREA ROLURILOR IN BD
                    context.Roles.AddRange(
                        new IdentityRole
                        {
                            Id = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                            Name = "Admin",
                            NormalizedName = "Admin".ToUpper()
                        },
                        new IdentityRole
                        {
                            Id = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                            Name = "User",
                            NormalizedName = "User".ToUpper()
                        }
                    );
                }

                if (!context.Users.Any())
                {
                    var hasher = new PasswordHasher<ApplicationUser>();

                    // CREAREA USERILOR IN BD
                    context.Users.AddRange(
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            UserName = "admin",
                            EmailConfirmed = true,
                            NormalizedEmail = "ADMIN@BATWITTER.COM",
                            Email = "admin@batwitter.com",
                            NormalizedUserName = "ADMIN",
                            PasswordHash = hasher.HashPassword(null, "Admin1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                            UserName = "deleted",
                            EmailConfirmed = true,
                            NormalizedEmail = "DELETED@BATWITTER.COM",
                            Email = "deleted@batwitter.com",
                            NormalizedUserName = "DELETED",
                            PasswordHash = hasher.HashPassword(null, "Deleted1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                            UserName = "alexn_20",
                            EmailConfirmed = true,
                            NormalizedEmail = "ALEXNED2004@GMAIL.COM",
                            Email = "alexned2004@gmail.com",
                            NormalizedUserName = "ALEXN_20",
                            PasswordHash = hasher.HashPassword(null, "Alex1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb3",
                            UserName = "kyogre36",
                            EmailConfirmed = true,
                            NormalizedEmail = "IOANSANDULESCU@GMAIL.COM",
                            Email = "ioansandulescu@gmail.com",
                            NormalizedUserName = "KYOGRE36",
                            PasswordHash = hasher.HashPassword(null, "Ioan1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                            UserName = "stefan_cozma",
                            EmailConfirmed = true,
                            NormalizedEmail = "SCOZMA48@GMAIL.COM",
                            Email = "scozma48@gmail.com",
                            NormalizedUserName = "STEFAN_COZMA",
                            PasswordHash = hasher.HashPassword(null, "Stefan1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb5",
                            UserName = "alexia_nd",
                            EmailConfirmed = true,
                            NormalizedEmail = "ALEXIANED2010@GMAIL.COM",
                            Email = "alexianed2010@gmail.com",
                            NormalizedUserName = "ALEXIA_ND",
                            PasswordHash = hasher.HashPassword(null, "Alexia1!")
                        },

                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb6",
                            UserName = "dianap",
                            EmailConfirmed = true,
                            NormalizedEmail = "DIANAPOPESCU@GMAIL.COM",
                            Email = "dianapopescu@gmail.com",
                            NormalizedUserName = "DIANAP",
                            PasswordHash = hasher.HashPassword(null, "Diana1!")
                        }
                    );

                    // ASOCIEREA USER-ROLE
                    context.UserRoles.AddRange(
                        new IdentityUserRole<string>
                        {
                            RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                        },
                        new IdentityUserRole<string>
                        {
                            RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                        },
                        new IdentityUserRole<string>
                        {
                            RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                        },
                        new IdentityUserRole<string>
                        {
                            RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3"
                        },
                        new IdentityUserRole<string>
                        {
                            RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4"
                        },
                        new IdentityUserRole<string>
                        {
                            RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5"
                        },
                        new IdentityUserRole<string>
                        {
                            RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb6"
                        }

                    );
                }

                if (!context.UserProfiles.Any())
                {
                    // CREAREA USERPROFILES PENTRU UTILIZATORII INITIALI
                    context.UserProfiles.AddRange(
                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb0", // admin (deleted)
                            DisplayName = "Admin",
                            JoinDate = DateTime.Now,
                            AccountStatus = "active"
                        },
                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb1", // deleted
                            DisplayName = "Deleted User",
                            JoinDate = DateTime.Now,
                            AccountStatus = "deleted"
                        },
                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                            DisplayName = "Alex Nedelcu",
                            JoinDate = DateTime.Now,
                            AccountStatus = "active"
                        },
                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2)
                            DisplayName = "Ioan Sandulescu",
                            JoinDate = DateTime.Now,
                            AccountStatus = "active"
                        },
                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3 - private)
                            DisplayName = "Stefan Cozma",
                            JoinDate = DateTime.Now,
                            AccountStatus = "private"
                        },
                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb5", // alexia_nd (u4)
                            DisplayName = "Alexia Nedelcu",
                            JoinDate = DateTime.Now,
                            AccountStatus = "active"
                        },

                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb6", // dianap
                            DisplayName = "Diana Popescu",
                            JoinDate = DateTime.Now,
                            AccountStatus = "active"
                        }
                    );
                }

                if (!context.Relations.Any())
                {
                    // CREAREA RELATIONS (FOLLOWS & BLOCKS)
                    context.Relations.AddRange(
                        // FOLLOWS: u1 -> u3
                        new Relation
                        {
                            SenderId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                            ReceiverId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                            Type = 1, // followed
                            RelationDate = DateTime.Now
                        },
                        // FOLLOWS: u2 -> u3
                        new Relation
                        {
                            SenderId = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2)
                            ReceiverId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                            Type = 1, // followed
                            RelationDate = DateTime.Now
                        },
                        // FOLLOWS: u3 -> u4
                        new Relation
                        {
                            SenderId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                            ReceiverId = "8e445865-a24d-4543-a6c6-9443d048cdb5", // alexia_nd (u4)
                            Type = 1, // followed
                            RelationDate = DateTime.Now
                        },
                        // FOLLOWS: u4 -> u3
                        new Relation
                        {
                            SenderId = "8e445865-a24d-4543-a6c6-9443d048cdb5", // alexia_nd (u4)
                            ReceiverId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                            Type = 1, // followed
                            RelationDate = DateTime.Now
                        },
                        // BLOCKS: u2 -> u1
                        new Relation
                        {
                            SenderId = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2)
                            ReceiverId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                            Type = -1, // blocked
                            RelationDate = DateTime.Now
                        }
                    );
                }

                Flock? f1 = null, f2 = null, f3 = null, f4 = null;
                
                if (!context.Flocks.Any())
                {
                    // CREAREA FLOCKS (fără ID explicit - se generează automat)
                    f1 = new Flock
                    {
                        AdminId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                        Name = "Star Wars",
                        DateCreated = DateTime.Now,
                        FlockStatus = "active"
                    };
                    f2 = new Flock
                    {
                        AdminId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                        Name = "Football",
                        DateCreated = DateTime.Now,
                        FlockStatus = "active"
                    };
                    f3 = new Flock
                    {
                        AdminId = "8e445865-a24d-4543-a6c6-9443d048cdb0", // admin (deleted)
                        Name = "Music",
                        Description = "A place to discuss about music",
                        DateCreated = DateTime.Now,
                        FlockStatus = "active"
                    };
                    f4 = new Flock
                    {
                        AdminId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                        Name = "IT",
                        Description = "A place to discuss about IT news",
                        DateCreated = DateTime.Now,
                        FlockStatus = "active"
                    };
                    
                    context.Flocks.AddRange(f1, f2, f3, f4);
                    context.SaveChanges(); // Salvează pentru a obține ID-urile generate
                }
                else
                {
                    // Dacă există deja, obține Flocks existente
                    f1 = context.Flocks.FirstOrDefault(f => f.Name == "Star Wars");
                    f2 = context.Flocks.FirstOrDefault(f => f.Name == "Football");
                    f3 = context.Flocks.FirstOrDefault(f => f.Name == "Music");
                    f4 = context.Flocks.FirstOrDefault(f => f.Name == "IT");
                }

                if (!context.FlockUsers.Any() && f1 != null && f2 != null && f3 != null && f4 != null)
                {
                    // CREAREA FLOCK USERS
                    // f1 - u1 (flockAdmin), u3
                    context.FlockUsers.AddRange(
                        new FlockUser
                        {
                            FlockId = f1.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                            Role = "admin",
                            JoinDate = DateTime.Now
                        },
                        new FlockUser
                        {
                            FlockId = f1.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                            Role = "member",
                            JoinDate = DateTime.Now
                        },
                        // f2 - u3 (flockAdmin), u2, u4
                        new FlockUser
                        {
                            FlockId = f2.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                            Role = "admin",
                            JoinDate = DateTime.Now
                        },
                        new FlockUser
                        {
                            FlockId = f2.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2)
                            Role = "member",
                            JoinDate = DateTime.Now
                        },
                        new FlockUser
                        {
                            FlockId = f2.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5", // alexia_nd (u4)
                            Role = "member",
                            JoinDate = DateTime.Now
                        },
                        // f3 - admin (flockAdmin), u1, u2, u3
                        new FlockUser
                        {
                            FlockId = f3.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0", // admin
                            Role = "admin",
                            JoinDate = DateTime.Now
                        },
                        new FlockUser
                        {
                            FlockId = f3.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                            Role = "member",
                            JoinDate = DateTime.Now
                        },
                        new FlockUser
                        {
                            FlockId = f3.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2)
                            Role = "member",
                            JoinDate = DateTime.Now
                        },
                        new FlockUser
                        {
                            FlockId = f3.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                            Role = "member",
                            JoinDate = DateTime.Now
                        },
                        // f4 - u1 (flockAdmin), u1, u2, u3, u4
                        new FlockUser
                        {
                            FlockId = f4.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                            Role = "admin",
                            JoinDate = DateTime.Now
                        },
                        new FlockUser
                        {
                            FlockId = f4.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2)
                            Role = "member",
                            JoinDate = DateTime.Now
                        },
                        new FlockUser
                        {
                            FlockId = f4.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                            Role = "member",
                            JoinDate = DateTime.Now
                        },
                        new FlockUser
                        {
                            FlockId = f4.Id,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5", // alexia_nd (u4)
                            Role = "member",
                            JoinDate = DateTime.Now
                        }
                    );
                }

                if (!context.Echoes.Any())
                {
                    // CREAREA ECHOES (fără ID explicit - se generează automat)
                    // e1 -> u1 att1 att2 content
                    var e1 = new Echo
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                        Content = "Hello there!",
                        FlockId = null,
                        CommParentId = null,
                        AmpParentId = null,
                        DateCreated = DateTime.Now,
                        IsRemoved = false
                    };
                    context.Echoes.Add(e1);
                    context.SaveChanges(); // Salvează pentru a obține ID-ul generat
                    
                    // e2 -> u1 content
                    var e2 = new Echo
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                        Content = "What's up?",
                        Att1 = null,
                        Att2 = null,
                        FlockId = null,
                        CommParentId = null,
                        AmpParentId = null,
                        DateCreated = DateTime.Now,
                        IsRemoved = false
                    };
                    context.Echoes.Add(e2);
                    context.SaveChanges();
                    
                    // e3 -> u2 comentariu la e1
                    var e3 = new Echo
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2)
                        Content = "What a great day!!!",
                        Att1 = null,
                        Att2 = null,
                        FlockId = null,
                        CommParentId = e1.Id, // e1
                        AmpParentId = null,
                        DateCreated = DateTime.Now,
                        IsRemoved = false
                    };
                    context.Echoes.Add(e3);
                    context.SaveChanges();
                    // Actualizează CommentsCount pentru e1
                    e1.CommentsCount++;
                    context.Echoes.Update(e1);
                    context.SaveChanges();
                    
                    // e4 -> u3 comentariu la e3
                    var e4 = new Echo
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                        Content = "Eh...not really",
                        Att1 = null,
                        Att2 = null,
                        FlockId = null,
                        CommParentId = e3.Id, // e3
                        AmpParentId = null,
                        DateCreated = DateTime.Now,
                        IsRemoved = false
                    };
                    context.Echoes.Add(e4);
                    context.SaveChanges();
                    // Actualizează CommentsCount pentru e3
                    e3.CommentsCount++;
                    context.Echoes.Update(e3);
                    context.SaveChanges();
                    
                    // e5 -> u4 comentariu la e1
                    var e5 = new Echo
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5", // alexia_nd (u4)
                        Content = "What's up bro?",
                        Att1 = null,
                        Att2 = null,
                        FlockId = null,
                        CommParentId = e1.Id, // e1
                        AmpParentId = null,
                        DateCreated = DateTime.Now,
                        IsRemoved = false
                    };
                    context.Echoes.Add(e5);
                    context.SaveChanges();
                    // Actualizează CommentsCount pentru e1
                    e1.CommentsCount++;
                    context.Echoes.Update(e1);
                    context.SaveChanges();
                    
                    // e6 -> u2 content (flock id f2)
                    var e6 = new Echo
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2)
                        Content = "Isn't football great?",
                        Att1 = null,
                        Att2 = null,
                        FlockId = f2?.Id, // f2
                        CommParentId = null,
                        AmpParentId = null,
                        DateCreated = DateTime.Now,
                        IsRemoved = false
                    };
                    context.Echoes.Add(e6);
                    context.SaveChanges();
                    
                    // e7 -> u3 att1
                    var e7 = new Echo
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                        Content = "Seeesh, I'm exhausted. Had a very long day.",
                        Att1 = null,
                        Att2 = null,
                        FlockId = null,
                        CommParentId = null,
                        AmpParentId = null,
                        DateCreated = DateTime.Now,
                        IsRemoved = false
                    };
                    context.Echoes.Add(e7);
                    context.SaveChanges();
                    
                    // e8 -> comment u7
                    var e8 = new Echo
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                        Content = "Me too :(",
                        Att1 = null,
                        Att2 = null,
                        FlockId = null,
                        CommParentId = e7.Id, // e7
                        AmpParentId = null,
                        DateCreated = DateTime.Now,
                        IsRemoved = false
                    };
                    context.Echoes.Add(e8);
                    context.SaveChanges();
                    // Actualizează CommentsCount pentru e7
                    e7.CommentsCount++;
                    context.Echoes.Update(e7);
                    context.SaveChanges();
                    
                    // e9 -> amplifier e7
                    var e9 = new Echo
                    {
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                        Content = "Could be the title of my autobiography",
                        Att1 = null,
                        Att2 = null,
                        FlockId = null,
                        CommParentId = null,
                        AmpParentId = e7.Id, // e7
                        DateCreated = DateTime.Now,
                        IsRemoved = false
                    };
                    context.Echoes.Add(e9);
                    context.SaveChanges();
                    // Actualizează AmplifierCount pentru e7
                    e7.AmplifierCount++;
                    context.Echoes.Update(e7);
                    context.SaveChanges();
                }

                if (!context.Interactions.Any())
                {
                    // CREAREA INTERACTIONS (likes, bookmarks, rebounds)
                    // Obține doar ID-urile echo-urilor fără tracking
                    var e1Id = context.Echoes.AsNoTracking().FirstOrDefault(e => e.Content == "Hello there!")?.Id;
                    var e2Id = context.Echoes.AsNoTracking().FirstOrDefault(e => e.Content == "What's up?")?.Id;
                    var e6Id = context.Echoes.AsNoTracking().FirstOrDefault(e => e.Content == "Isn't football great?")?.Id;
                    var e7Id = context.Echoes.AsNoTracking().FirstOrDefault(e => e.Content == "Seeesh, I'm exhausted. Had a very long day.")?.Id;

                    if (e1Id.HasValue && e2Id.HasValue && e6Id.HasValue && e7Id.HasValue)
                    {
                        // Șterge tracking-ul pentru a evita conflicte
                        context.ChangeTracker.Clear();
                        
                        context.Interactions.AddRange(
                            // Likes pe e1
                            new Interaction
                            {
                                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2) - like + bookmark
                                EchoId = e1Id.Value,
                                Liked = true,
                                Bookmarked = true,
                                LikedDate = DateTime.Now,
                                BookmarkedDate = DateTime.Now
                            },
                            new Interaction
                            {
                                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4", // stefan_cozma (u3)
                                EchoId = e1Id.Value,
                                Liked = true,
                                LikedDate = DateTime.Now
                            },
                            new Interaction
                            {
                                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5", // alexia_nd (u4)
                                EchoId = e1Id.Value,
                                Liked = true,
                                LikedDate = DateTime.Now
                            },
                            // Likes pe e2
                            new Interaction
                            {
                                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", // kyogre36 (u2)
                                EchoId = e2Id.Value,
                                Liked = true,
                                LikedDate = DateTime.Now
                            },
                            // Likes pe e7
                            new Interaction
                            {
                                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                                EchoId = e7Id.Value,
                                Liked = true,
                                LikedDate = DateTime.Now
                            },
                            new Interaction
                            {
                                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb5", // alexia_nd (u4)
                                EchoId = e7Id.Value,
                                Liked = true,
                                LikedDate = DateTime.Now
                            },
                            // Bookmarks
                            new Interaction
                            {
                                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", // alexn_20 (u1)
                                EchoId = e6Id.Value,
                                Bookmarked = true,
                                BookmarkedDate = DateTime.Now
                            }
                        );
                        context.SaveChanges();

                        // Actualizează LikesCount și BookmarksCount folosind ExecuteSqlRaw pentru a evita tracking issues
                        context.Database.ExecuteSqlRaw(
                            $"UPDATE Echoes SET LikesCount = 3, BookmarksCount = 1 WHERE Id = {e1Id.Value}; " +
                            $"UPDATE Echoes SET LikesCount = 1 WHERE Id = {e2Id.Value}; " +
                            $"UPDATE Echoes SET BookmarksCount = 1 WHERE Id = {e6Id.Value}; " +
                            $"UPDATE Echoes SET LikesCount = 2 WHERE Id = {e7Id.Value};"
                        );
                    }
                }

                context.SaveChanges();
            }
        }
    }
}