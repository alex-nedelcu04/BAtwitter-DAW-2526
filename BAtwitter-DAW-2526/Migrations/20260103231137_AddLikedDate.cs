using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAtwitter_DAW_2526.Migrations
{
    /// <inheritdoc />
    public partial class AddLikedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LikedDate",
                table: "Interactions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LikedDate",
                table: "Interactions");
        }
    }
}
