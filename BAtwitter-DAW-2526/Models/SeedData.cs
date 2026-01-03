using BAtwitter_DAW_2526.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace BAtwitter_DAW_2526.Models
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
                    // daca nu contine roluri, acestea se vor crea
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
                            Name = "Editor",
                            NormalizedName = "Editor".ToUpper()
                        },

                        new IdentityRole
                        {
                            Id = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                            Name = "User",
                            NormalizedName = "User".ToUpper()
                        }
                    );
                }

                if (!context.Users.Any())
                {
                    // o noua instanta pe care o vom utiliza pentru crearea parolelor utilizatorilor
                    // parolele sunt de tip hash
                    var hasher = new PasswordHasher<ApplicationUser>();

                    // CREAREA USERILOR IN BD
                    // Se creeaza cate un user pentru fiecare rol
                    context.Users.AddRange(
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            // primary key
                            UserName = "admin_test",
                            EmailConfirmed = true,
                            NormalizedEmail = "ADMIN@TEST.COM",
                            Email = "admin@test.com",
                            NormalizedUserName = "ADMIN_TEST",
                            PasswordHash = hasher.HashPassword(null, "Admin1!")
                        },

                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                            // primary key
                            UserName = "editor_test",
                            EmailConfirmed = true,
                            NormalizedEmail = "EDITOR@TEST.COM",
                            Email = "editor@test.com",
                            NormalizedUserName = "EDITOR_TEST",
                            PasswordHash = hasher.HashPassword(null, "Editor1!")
                        },

                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                            // primary key
                            UserName = "user_test",
                            EmailConfirmed = true,
                            NormalizedEmail = "USER@TEST.COM",
                            Email = "user@test.com",
                            NormalizedUserName = "USER_TEST",
                            PasswordHash = hasher.HashPassword(null, "User1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb3",
                            // primary key - utilizator pentru postările șterse
                            UserName = "deleted",
                            EmailConfirmed = true,
                            NormalizedEmail = "DELETED@SYSTEM.COM",
                            Email = "deleted@system.com",
                            NormalizedUserName = "DELETED",
                            PasswordHash = hasher.HashPassword(null, "Deleted1!")
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
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3"
                    }
                    );
                }

               

                if (!context.UserProfiles.Any())
                {
                    // CREAREA USERPROFILES PENTRU UTILIZATORII INITIALI
                    context.UserProfiles.AddRange(
                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb0", // ApplicationUserId
                            DisplayName = "Admin User",
                            //PfpLink = "/Resources/Images/user_default_pfp.jpg",
                            //BannerLink = "/Resources/Images/banner-default.jpg",
                            JoinDate = DateTime.Now,
                            AccountStatus = "active"
                        },
                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb1", // ApplicationUserId
                            DisplayName = "Editor User",
                            //PfpLink = "/Resources/Images/user_default_pfp.jpg",
                            //BannerLink = "/Resources/Images/banner-default.jpg",
                            JoinDate = DateTime.Now,
                            AccountStatus = "active"
                        },
                        new UserProfile
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb2", // ApplicationUserId
                            DisplayName = "Regular User",
                            //PfpLink = "/Resources/Images/user_default_pfp.jpg",
                            //BannerLink = "/Resources/Images/banner-default.jpg",
                            JoinDate = DateTime.Now,
                            AccountStatus = "active"
                        },

                         new UserProfile
                         {
                             Id = "8e445865-a24d-4543-a6c6-9443d048cdb3", // ApplicationUserId - utilizator pentru postările șterse
                             DisplayName = "Deleted User",
                             //PfpLink = "/Resources/Images/user_default_pfp.jpg",
                             //BannerLink = "/Resources/Images/banner-default.jpg",
                             JoinDate = DateTime.Now,
                             AccountStatus = "deleted"
                         }
                    );
                }

                context.SaveChanges();
            }
        }
    }
}
