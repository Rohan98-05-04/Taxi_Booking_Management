using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxi_Booking_Management.Migrations
{
    /// <inheritdoc />
    public partial class addLocationFieldNBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromLocation",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToLocation",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromLocation",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ToLocation",
                table: "Bookings");
        }
    }
}
