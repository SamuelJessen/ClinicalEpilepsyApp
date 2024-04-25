using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalEpilepsyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EcgProcessedMeasurements",
                columns: table => new
                {
                    ProcessedMeasurementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProcessedEcgChannel1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedEcgChannel2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedEcgChannel3 = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcgProcessedMeasurements", x => x.ProcessedMeasurementId);
                    table.ForeignKey(
                        name: "FK_EcgProcessedMeasurements_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EcgAlarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EcgProcessedMeasurementProcessedMeasurementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CSI30 = table.Column<int>(type: "int", nullable: false),
                    CSI50 = table.Column<int>(type: "int", nullable: false),
                    CSI100 = table.Column<int>(type: "int", nullable: false),
                    ModCSI30 = table.Column<int>(type: "int", nullable: false),
                    ModCSI50 = table.Column<int>(type: "int", nullable: false),
                    ModCSI100 = table.Column<int>(type: "int", nullable: false),
                    PatientCSIThreshold = table.Column<int>(type: "int", nullable: false),
                    PatientModCSIThreshold = table.Column<int>(type: "int", nullable: false),
                    AlarmTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcgAlarms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EcgAlarms_EcgProcessedMeasurements_EcgProcessedMeasurementProcessedMeasurementId",
                        column: x => x.EcgProcessedMeasurementProcessedMeasurementId,
                        principalTable: "EcgProcessedMeasurements",
                        principalColumn: "ProcessedMeasurementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EcgAlarms_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EcgAlarms_EcgProcessedMeasurementProcessedMeasurementId",
                table: "EcgAlarms",
                column: "EcgProcessedMeasurementProcessedMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_EcgAlarms_PatientId",
                table: "EcgAlarms",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_EcgProcessedMeasurements_PatientId",
                table: "EcgProcessedMeasurements",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EcgAlarms");

            migrationBuilder.DropTable(
                name: "EcgProcessedMeasurements");
        }
    }
}
