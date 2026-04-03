using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreJudge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemLanguageTemplatesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartingPoint",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "UserCodeTemplate",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "UserCodeWrapper",
                table: "Problems");

            migrationBuilder.CreateTable(
                name: "ProblemLangeuageTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserCodeTemplate = table.Column<string>(type: "text", nullable: false),
                    UserCodeWrapper = table.Column<string>(type: "text", nullable: false),
                    StartingPoint = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    ProblemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemLangeuageTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProblemLangeuageTemplates_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProblemLangeuageTemplates_ProblemId",
                table: "ProblemLangeuageTemplates",
                column: "ProblemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProblemLangeuageTemplates");

            migrationBuilder.AddColumn<string>(
                name: "StartingPoint",
                table: "Problems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserCodeTemplate",
                table: "Problems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserCodeWrapper",
                table: "Problems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
