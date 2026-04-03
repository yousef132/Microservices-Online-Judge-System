using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreJudge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStartingPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StartingPoint",
                table: "Problems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartingPoint",
                table: "Problems");
        }
    }
}
