using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAtwitter_DAW_2526.Migrations
{
    /// <inheritdoc />
    public partial class LastMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PfpLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pronouns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Flocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PfpLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FlockStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flocks_UserProfiles_AdminId",
                        column: x => x.AdminId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Relations",
                columns: table => new
                {
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RelationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relations", x => new { x.SenderId, x.ReceiverId });
                    table.ForeignKey(
                        name: "FK_Relations_UserProfiles_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Relations_UserProfiles_SenderId",
                        column: x => x.SenderId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Echoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlockId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommParentId = table.Column<int>(type: "int", nullable: true),
                    AmpParentId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Att1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Att2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LikesCount = table.Column<int>(type: "int", nullable: false),
                    CommentsCount = table.Column<int>(type: "int", nullable: false),
                    ReboundCount = table.Column<int>(type: "int", nullable: false),
                    AmplifierCount = table.Column<int>(type: "int", nullable: false),
                    BookmarksCount = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEdited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Echoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Echoes_Echoes_AmpParentId",
                        column: x => x.AmpParentId,
                        principalTable: "Echoes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Echoes_Echoes_CommParentId",
                        column: x => x.CommParentId,
                        principalTable: "Echoes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Echoes_Flocks_FlockId",
                        column: x => x.FlockId,
                        principalTable: "Flocks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Echoes_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlockUsers",
                columns: table => new
                {
                    FlockId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlockUsers", x => new { x.FlockId, x.UserId });
                    table.ForeignKey(
                        name: "FK_FlockUsers_Flocks_FlockId",
                        column: x => x.FlockId,
                        principalTable: "Flocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlockUsers_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FollowRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiverUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiverFlockId = table.Column<int>(type: "int", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowRequests_Flocks_ReceiverFlockId",
                        column: x => x.ReceiverFlockId,
                        principalTable: "Flocks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FollowRequests_UserProfiles_ReceiverUserId",
                        column: x => x.ReceiverUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FollowRequests_UserProfiles_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EchoId = table.Column<int>(type: "int", nullable: false),
                    AddDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => new { x.UserId, x.EchoId });
                    table.ForeignKey(
                        name: "FK_Bookmarks_Echoes_EchoId",
                        column: x => x.EchoId,
                        principalTable: "Echoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookmarks_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Interactions",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EchoId = table.Column<int>(type: "int", nullable: false),
                    Liked = table.Column<bool>(type: "bit", nullable: false),
                    Bookmarked = table.Column<bool>(type: "bit", nullable: false),
                    Rebounded = table.Column<bool>(type: "bit", nullable: false),
                    LikedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReboundedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookmarkedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactions", x => new { x.UserId, x.EchoId });
                    table.ForeignKey(
                        name: "FK_Interactions_Echoes_EchoId",
                        column: x => x.EchoId,
                        principalTable: "Echoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Interactions_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserName",
                table: "AspNetUsers",
                column: "UserName",
                unique: true,
                filter: "[UserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_EchoId",
                table: "Bookmarks",
                column: "EchoId");

            migrationBuilder.CreateIndex(
                name: "IX_Echoes_AmpParentId",
                table: "Echoes",
                column: "AmpParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Echoes_CommParentId",
                table: "Echoes",
                column: "CommParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Echoes_FlockId",
                table: "Echoes",
                column: "FlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Echoes_UserId",
                table: "Echoes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Flocks_AdminId",
                table: "Flocks",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_FlockUsers_UserId",
                table: "FlockUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_ReceiverFlockId",
                table: "FollowRequests",
                column: "ReceiverFlockId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_ReceiverUserId",
                table: "FollowRequests",
                column: "ReceiverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_SenderUserId_ReceiverFlockId",
                table: "FollowRequests",
                columns: new[] { "SenderUserId", "ReceiverFlockId" },
                unique: true,
                filter: "[ReceiverFlockId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_SenderUserId_ReceiverUserId",
                table: "FollowRequests",
                columns: new[] { "SenderUserId", "ReceiverUserId" },
                unique: true,
                filter: "[ReceiverFlockId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_EchoId",
                table: "Interactions",
                column: "EchoId");

            migrationBuilder.CreateIndex(
                name: "IX_Relations_ReceiverId",
                table: "Relations",
                column: "ReceiverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Bookmarks");

            migrationBuilder.DropTable(
                name: "FlockUsers");

            migrationBuilder.DropTable(
                name: "FollowRequests");

            migrationBuilder.DropTable(
                name: "Interactions");

            migrationBuilder.DropTable(
                name: "Relations");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Echoes");

            migrationBuilder.DropTable(
                name: "Flocks");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
