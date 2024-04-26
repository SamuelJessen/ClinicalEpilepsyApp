using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalEpilepsyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EcgAlarms_EcgProcessedMeasurements_EcgProcessedMeasurementProcessedMeasurementId",
                table: "EcgAlarms");

            migrationBuilder.DropForeignKey(
                name: "FK_EcgAlarms_Patients_PatientId",
                table: "EcgAlarms");

            migrationBuilder.DropForeignKey(
                name: "FK_EcgProcessedMeasurements_Patients_PatientId",
                table: "EcgProcessedMeasurements");

            migrationBuilder.DropIndex(
                name: "IX_EcgProcessedMeasurements_PatientId",
                table: "EcgProcessedMeasurements");

            migrationBuilder.DropIndex(
                name: "IX_EcgAlarms_EcgProcessedMeasurementProcessedMeasurementId",
                table: "EcgAlarms");

            migrationBuilder.DropIndex(
                name: "IX_EcgAlarms_PatientId",
                table: "EcgAlarms");

            migrationBuilder.RenameColumn(
                name: "EcgProcessedMeasurementProcessedMeasurementId",
                table: "EcgAlarms",
                newName: "EcgProcessedMeasurementId");

            migrationBuilder.AlterColumn<string>(
                name: "PatientId",
                table: "EcgProcessedMeasurements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PatientId",
                table: "EcgAlarms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EcgProcessedMeasurementId",
                table: "EcgAlarms",
                newName: "EcgProcessedMeasurementProcessedMeasurementId");

            migrationBuilder.AlterColumn<string>(
                name: "PatientId",
                table: "EcgProcessedMeasurements",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PatientId",
                table: "EcgAlarms",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_EcgProcessedMeasurements_PatientId",
                table: "EcgProcessedMeasurements",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_EcgAlarms_EcgProcessedMeasurementProcessedMeasurementId",
                table: "EcgAlarms",
                column: "EcgProcessedMeasurementProcessedMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_EcgAlarms_PatientId",
                table: "EcgAlarms",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_EcgAlarms_EcgProcessedMeasurements_EcgProcessedMeasurementProcessedMeasurementId",
                table: "EcgAlarms",
                column: "EcgProcessedMeasurementProcessedMeasurementId",
                principalTable: "EcgProcessedMeasurements",
                principalColumn: "ProcessedMeasurementId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EcgAlarms_Patients_PatientId",
                table: "EcgAlarms",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EcgProcessedMeasurements_Patients_PatientId",
                table: "EcgProcessedMeasurements",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }
    }
}
