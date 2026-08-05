using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.api.Migrations
{
    /// <inheritdoc />
    public partial class CompanyModelwebsitefieldnamemodified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "website",
                table: "Companies",
                newName: "Website");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Website",
                table: "Companies",
                newName: "website");
        }
    }
}
