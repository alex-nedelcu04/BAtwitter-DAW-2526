using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAtwitter_DAW_2526.Data.Migrations
{
    /// <inheritdoc />
    public partial class newestest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PfpLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pronouns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
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
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PfpLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    relationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CommParentId = table.Column<int>(type: "int", nullable: true),
                    AmpParentId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Att1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Att2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LikesCount = table.Column<int>(type: "int", nullable: false),
                    CommentsCount = table.Column<int>(type: "int", nullable: false),
                    ReboundCount = table.Column<int>(type: "int", nullable: false),
                    AmplifierCount = table.Column<int>(type: "int", nullable: false),
                    BookmarksCount = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    UserId = table.Column<int>(type: "int", nullable: false),
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
                name: "Bookmarks",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
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
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EchoId = table.Column<int>(type: "int", nullable: false),
                    Liked = table.Column<bool>(type: "bit", nullable: false),
                    Bookmarked = table.Column<bool>(type: "bit", nullable: false),
                    Rebounded = table.Column<bool>(type: "bit", nullable: false),
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
                name: "IX_Interactions_EchoId",
                table: "Interactions",
                column: "EchoId");

            migrationBuilder.CreateIndex(
                name: "IX_Relations_ReceiverId",
                table: "Relations",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_ApplicationUserId",
                table: "UserProfiles",
                column: "ApplicationUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookmarks");

            migrationBuilder.DropTable(
                name: "FlockUsers");

            migrationBuilder.DropTable(
                name: "Interactions");

            migrationBuilder.DropTable(
                name: "Relations");

            migrationBuilder.DropTable(
                name: "Echoes");

            migrationBuilder.DropTable(
                name: "Flocks");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
