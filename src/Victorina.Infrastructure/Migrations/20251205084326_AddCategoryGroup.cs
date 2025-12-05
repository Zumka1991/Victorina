using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Victorina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryGroup",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryGroup",
                table: "Categories");
        }
    }
}
