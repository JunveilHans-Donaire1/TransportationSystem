using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsTournament.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDetail",
                table: "UserDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Records",
                table: "Records");

            migrationBuilder.RenameTable(
                name: "UserDetail",
                newName: "UserDetails");

            migrationBuilder.RenameTable(
                name: "Records",
                newName: "TransportationRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "TransportationRecords",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Username",
                table: "Users",
                column: "Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDetails",
                table: "UserDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransportationRecords",
                table: "TransportationRecords",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TransportationRecords_Username",
                table: "TransportationRecords",
                column: "Username");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportationRecords_Users_Username",
                table: "TransportationRecords",
                column: "Username",
                principalTable: "Users",
                principalColumn: "Username",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportationRecords_Users_Username",
                table: "TransportationRecords");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Username",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDetails",
                table: "UserDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransportationRecords",
                table: "TransportationRecords");

            migrationBuilder.DropIndex(
                name: "IX_TransportationRecords_Username",
                table: "TransportationRecords");

            migrationBuilder.RenameTable(
                name: "UserDetails",
                newName: "UserDetail");

            migrationBuilder.RenameTable(
                name: "TransportationRecords",
                newName: "Records");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Records",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDetail",
                table: "UserDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Records",
                table: "Records",
                column: "Id");
        }
    }
}
