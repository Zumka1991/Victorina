using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Victorina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserQuestionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserQuestionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    QuestionTranslationGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShownAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuestionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuestionHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionHistories_ShownAt",
                table: "UserQuestionHistories",
                column: "ShownAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionHistories_UserId_QuestionTranslationGroupId",
                table: "UserQuestionHistories",
                columns: new[] { "UserId", "QuestionTranslationGroupId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserQuestionHistories");
        }
    }
}
