using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreJudge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RanmeExecutinoTimeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubmitTime",
                table: "Submissions",
                newName: "MaxExecutionTimeMs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxExecutionTimeMs",
                table: "Submissions",
                newName: "SubmitTime");
        }
    }
}
