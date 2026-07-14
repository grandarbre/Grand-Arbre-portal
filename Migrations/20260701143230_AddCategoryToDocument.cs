using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grand_Arbre_portal.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Documents");
        }
    }
}
