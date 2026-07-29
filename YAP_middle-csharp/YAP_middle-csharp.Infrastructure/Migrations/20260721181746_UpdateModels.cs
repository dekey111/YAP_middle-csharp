using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YAP_middle_csharp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Login = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    UserRole = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "Login",
                unique: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Login", "PasswordHash", "UserRole" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "system_legacy_user", "legacy_hash", "User" });

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"Bookings\" SET \"UserId\" = '11111111-1111-1111-1111-111111111111' WHERE \"UserId\" IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Bookings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
