using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxi_Booking_Management.Migrations
{
    /// <inheritdoc />
    public partial class addSoftdelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "taxis",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "owner",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "drivers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "taxis");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "owner");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "drivers");
        }
    }
}
