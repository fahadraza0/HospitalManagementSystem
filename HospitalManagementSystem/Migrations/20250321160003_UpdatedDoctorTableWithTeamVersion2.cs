using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagementSystem.Migrations
{
    public partial class UpdatedDoctorTableWithTeamVersion2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorTeam");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "Teams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Doctors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_DoctorId",
                table: "Teams",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_TeamId",
                table: "Doctors",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Teams_TeamId",
                table: "Doctors",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Doctors_DoctorId",
                table: "Teams",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Teams_TeamId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Doctors_DoctorId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_DoctorId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_TeamId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Doctors");

            migrationBuilder.CreateTable(
                name: "DoctorTeam",
                columns: table => new
                {
                    DoctorsDoctorId = table.Column<int>(type: "int", nullable: false),
                    TeamsTeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorTeam", x => new { x.DoctorsDoctorId, x.TeamsTeamId });
                    table.ForeignKey(
                        name: "FK_DoctorTeam_Doctors_DoctorsDoctorId",
                        column: x => x.DoctorsDoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorTeam_Teams_TeamsTeamId",
                        column: x => x.TeamsTeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorTeam_TeamsTeamId",
                table: "DoctorTeam",
                column: "TeamsTeamId");
        }
    }
}
