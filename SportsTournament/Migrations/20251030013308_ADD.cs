using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsTournament.Migrations
{
    /// <inheritdoc />
    public partial class ADD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentRecords",
                table: "TournamentRecords");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TournamentRecords");

            migrationBuilder.RenameTable(
                name: "TournamentRecords",
                newName: "Records");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Records",
                table: "Records",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Records",
                table: "Records");

            migrationBuilder.RenameTable(
                name: "Records",
                newName: "TournamentRecords");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TournamentRecords",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentRecords",
                table: "TournamentRecords",
                column: "Id");
        }
    }
}
