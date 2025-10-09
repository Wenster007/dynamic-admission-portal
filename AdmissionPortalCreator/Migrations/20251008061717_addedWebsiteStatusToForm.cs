using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdmissionPortalCreator.Migrations
{
    /// <inheritdoc />
    public partial class addedWebsiteStatusToForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationWebsite",
                table: "Forms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Forms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationWebsite",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Forms");
        }
    }
}
