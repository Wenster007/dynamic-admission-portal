using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdmissionPortalCreator.Migrations
{
    /// <inheritdoc />
    public partial class changedTenantWebsiteField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubdomainOrUrl",
                table: "Tenants",
                newName: "Website");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Website",
                table: "Tenants",
                newName: "SubdomainOrUrl");
        }
    }
}
