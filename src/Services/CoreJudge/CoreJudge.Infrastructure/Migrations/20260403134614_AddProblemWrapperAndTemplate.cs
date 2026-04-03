using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreJudge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemWrapperAndTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserCodeTemplate",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "UserCodeWrapper",
                table: "Problems");
        }
    }
}
