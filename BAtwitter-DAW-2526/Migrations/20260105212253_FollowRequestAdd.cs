using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAtwitter_DAW_2526.Migrations
{
    /// <inheritdoc />
    public partial class FollowRequestAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowRequests");
        }
    }
}
