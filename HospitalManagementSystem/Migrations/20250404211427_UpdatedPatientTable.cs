using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagementSystem.Migrations
{
    public partial class UpdatedPatientTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_Doctors_DoctorId",
                table: "Billings");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "Billings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Billings_DoctorId1",
                table: "Billings",
                column: "DoctorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_Doctors_DoctorId",
                table: "Billings",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_Doctors_DoctorId1",
                table: "Billings",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "DoctorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_Doctors_DoctorId",
                table: "Billings");

            migrationBuilder.DropForeignKey(
                name: "FK_Billings_Doctors_DoctorId1",
                table: "Billings");

            migrationBuilder.DropIndex(
                name: "IX_Billings_DoctorId1",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "Billings");

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_Doctors_DoctorId",
                table: "Billings",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
