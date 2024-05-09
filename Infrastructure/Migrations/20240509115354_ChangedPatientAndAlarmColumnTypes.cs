using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalEpilepsyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedPatientAndAlarmColumnTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModCSIThreshold",
                table: "Patients",
                newName: "ModCSIThreshold100");

            migrationBuilder.RenameColumn(
                name: "CSIThreshold",
                table: "Patients",
                newName: "CSIThreshold50");

            migrationBuilder.RenameColumn(
                name: "PatientModCSIThreshold",
                table: "EcgAlarms",
                newName: "PatientModCSIThreshold100");

            migrationBuilder.RenameColumn(
                name: "PatientCSIThreshold",
                table: "EcgAlarms",
                newName: "PatientCSIThreshold50");

            migrationBuilder.AddColumn<int>(
                name: "CSIThreshold100",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CSIThreshold30",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CSI100Alarm",
                table: "EcgAlarms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CSI30Alarm",
                table: "EcgAlarms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CSI50Alarm",
                table: "EcgAlarms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ModCSI100Alarm",
                table: "EcgAlarms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PatientCSIThreshold100",
                table: "EcgAlarms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PatientCSIThreshold30",
                table: "EcgAlarms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CSIThreshold100",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "CSIThreshold30",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "CSI100Alarm",
                table: "EcgAlarms");

            migrationBuilder.DropColumn(
                name: "CSI30Alarm",
                table: "EcgAlarms");

            migrationBuilder.DropColumn(
                name: "CSI50Alarm",
                table: "EcgAlarms");

            migrationBuilder.DropColumn(
                name: "ModCSI100Alarm",
                table: "EcgAlarms");

            migrationBuilder.DropColumn(
                name: "PatientCSIThreshold100",
                table: "EcgAlarms");

            migrationBuilder.DropColumn(
                name: "PatientCSIThreshold30",
                table: "EcgAlarms");

            migrationBuilder.RenameColumn(
                name: "ModCSIThreshold100",
                table: "Patients",
                newName: "ModCSIThreshold");

            migrationBuilder.RenameColumn(
                name: "CSIThreshold50",
                table: "Patients",
                newName: "CSIThreshold");

            migrationBuilder.RenameColumn(
                name: "PatientModCSIThreshold100",
                table: "EcgAlarms",
                newName: "PatientModCSIThreshold");

            migrationBuilder.RenameColumn(
                name: "PatientCSIThreshold50",
                table: "EcgAlarms",
                newName: "PatientCSIThreshold");
        }
    }
}
