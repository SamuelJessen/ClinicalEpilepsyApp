using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalEpilepsyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EcgAlarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EcgProcessedMeasurementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CSI30 = table.Column<int>(type: "int", nullable: false),
                    CSI50 = table.Column<int>(type: "int", nullable: false),
                    CSI100 = table.Column<int>(type: "int", nullable: false),
                    ModCSI100 = table.Column<int>(type: "int", nullable: false),
                    PatientCSIThreshold = table.Column<int>(type: "int", nullable: false),
                    PatientModCSIThreshold = table.Column<int>(type: "int", nullable: false),
                    AlarmTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcgAlarms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EcgProcessedMeasurements",
                columns: table => new
                {
                    ProcessedMeasurementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedEcgChannel1 = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ProcessedEcgChannel2 = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ProcessedEcgChannel3 = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcgProcessedMeasurements", x => x.ProcessedMeasurementId);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CSIThreshold = table.Column<int>(type: "int", nullable: false),
                    ModCSIThreshold = table.Column<int>(type: "int", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EcgAlarms");

            migrationBuilder.DropTable(
                name: "EcgProcessedMeasurements");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
