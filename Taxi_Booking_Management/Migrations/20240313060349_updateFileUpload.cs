using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxi_Booking_Management.Migrations
{
    /// <inheritdoc />
    public partial class updateFileUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "taxis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "owner",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "drivers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "taxis");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "owner");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "drivers");
        }
    }
}
