using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdmissionPortalCreator.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTenantIdFromRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AspNetRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
