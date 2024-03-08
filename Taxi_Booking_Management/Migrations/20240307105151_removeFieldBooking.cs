using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxi_Booking_Management.Migrations
{
    /// <inheritdoc />
    public partial class removeFieldBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remark",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
