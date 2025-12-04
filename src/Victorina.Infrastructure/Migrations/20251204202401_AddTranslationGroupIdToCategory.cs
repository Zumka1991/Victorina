using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Victorina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationGroupIdToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TranslationGroupId",
                table: "Categories",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranslationGroupId",
                table: "Categories");
        }
    }
}
