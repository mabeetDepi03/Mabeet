using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MabeetApi.Migrations
{
    /// <inheritdoc />
    public partial class fixAccBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Bookings",
                type: "DateTime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "HotelAccommodationID",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StudentHouseAccommodationID",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HotelAccommodationID",
                table: "Bookings",
                column: "HotelAccommodationID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StudentHouseAccommodationID",
                table: "Bookings",
                column: "StudentHouseAccommodationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Accommodations_HotelAccommodationID",
                table: "Bookings",
                column: "HotelAccommodationID",
                principalTable: "Accommodations",
                principalColumn: "AccommodationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Accommodations_StudentHouseAccommodationID",
                table: "Bookings",
                column: "StudentHouseAccommodationID",
                principalTable: "Accommodations",
                principalColumn: "AccommodationID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Accommodations_HotelAccommodationID",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Accommodations_StudentHouseAccommodationID",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_HotelAccommodationID",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StudentHouseAccommodationID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HotelAccommodationID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StudentHouseAccommodationID",
                table: "Bookings");
        }
    }
}
