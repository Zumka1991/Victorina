using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Victorina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageCodeToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ru");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Questions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ru");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Categories",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ru");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Categories");
        }
    }
}
