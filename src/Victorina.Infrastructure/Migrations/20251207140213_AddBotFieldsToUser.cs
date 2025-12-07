using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Victorina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBotFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BotDifficulty",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBot",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BotDifficulty",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBot",
                table: "Users");
        }
    }
}
