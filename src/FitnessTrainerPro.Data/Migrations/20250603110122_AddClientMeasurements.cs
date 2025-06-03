using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessTrainerPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientMeasurements",
                columns: table => new
                {
                    MeasurementID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientID = table.Column<int>(type: "INTEGER", nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WeightKg = table.Column<double>(type: "REAL", nullable: true),
                    ChestCm = table.Column<double>(type: "REAL", nullable: true),
                    WaistCm = table.Column<double>(type: "REAL", nullable: true),
                    HipsCm = table.Column<double>(type: "REAL", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientMeasurements", x => x.MeasurementID);
                    table.ForeignKey(
                        name: "FK_ClientMeasurements_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ClientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMeasurements_ClientID",
                table: "ClientMeasurements",
                column: "ClientID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientMeasurements");
        }
    }
}
