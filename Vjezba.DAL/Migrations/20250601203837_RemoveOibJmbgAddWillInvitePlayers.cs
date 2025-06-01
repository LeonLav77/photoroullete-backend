using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vjezba.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOibJmbgAddWillInvitePlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OIB",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "JMBG",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "WillInvitePlayers",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WillInvitePlayers",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "OIB",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JMBG",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 13,
                nullable: true);
        }
    }
}