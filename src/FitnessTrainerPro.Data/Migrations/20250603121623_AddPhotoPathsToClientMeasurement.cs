using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessTrainerPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoPathsToClientMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoAfterPath",
                table: "ClientMeasurements",
                type: "TEXT",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoBeforePath",
                table: "ClientMeasurements",
                type: "TEXT",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoAfterPath",
                table: "ClientMeasurements");

            migrationBuilder.DropColumn(
                name: "PhotoBeforePath",
                table: "ClientMeasurements");
        }
    }
}
