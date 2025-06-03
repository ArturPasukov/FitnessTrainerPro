using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessTrainerPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutProgramAndRelatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkoutPrograms",
                columns: table => new
                {
                    ProgramID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Focus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPrograms", x => x.ProgramID);
                });

            migrationBuilder.CreateTable(
                name: "ClientAssignedPrograms",
                columns: table => new
                {
                    ClientAssignedProgramID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgramID = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrainerNotesForClient = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAssignedPrograms", x => x.ClientAssignedProgramID);
                    table.ForeignKey(
                        name: "FK_ClientAssignedPrograms_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ClientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientAssignedPrograms_WorkoutPrograms_ProgramID",
                        column: x => x.ProgramID,
                        principalTable: "WorkoutPrograms",
                        principalColumn: "ProgramID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramExercises",
                columns: table => new
                {
                    ProgramExerciseID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProgramID = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseID = table.Column<int>(type: "INTEGER", nullable: false),
                    Sets = table.Column<int>(type: "INTEGER", nullable: false),
                    Reps = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RestSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    OrderInProgram = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramExercises", x => x.ProgramExerciseID);
                    table.ForeignKey(
                        name: "FK_ProgramExercises_Exercises_ExerciseID",
                        column: x => x.ExerciseID,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramExercises_WorkoutPrograms_ProgramID",
                        column: x => x.ProgramID,
                        principalTable: "WorkoutPrograms",
                        principalColumn: "ProgramID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssignedPrograms_ClientID",
                table: "ClientAssignedPrograms",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssignedPrograms_ProgramID",
                table: "ClientAssignedPrograms",
                column: "ProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramExercises_ExerciseID",
                table: "ProgramExercises",
                column: "ExerciseID");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramExercises_ProgramID",
                table: "ProgramExercises",
                column: "ProgramID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientAssignedPrograms");

            migrationBuilder.DropTable(
                name: "ProgramExercises");

            migrationBuilder.DropTable(
                name: "WorkoutPrograms");
        }
    }
}
