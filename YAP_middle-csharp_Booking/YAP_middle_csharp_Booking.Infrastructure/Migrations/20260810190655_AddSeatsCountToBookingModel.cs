using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YAP_middle_csharp_Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatsCountToBookingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeatsCount",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeatsCount",
                table: "Bookings");
        }
    }
}
