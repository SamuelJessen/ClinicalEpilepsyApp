using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalEpilepsyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePatientProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CSI",
                table: "Patients",
                newName: "ModCSIThreshold");

            migrationBuilder.AddColumn<int>(
                name: "CSIThreshold",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CSIThreshold",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "ModCSIThreshold",
                table: "Patients",
                newName: "CSI");
        }
    }
}
