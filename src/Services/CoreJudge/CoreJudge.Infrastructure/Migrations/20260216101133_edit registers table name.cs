using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreJudge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editregisterstablename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registers_Contests_ContestId",
                table: "Registers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Registers",
                table: "Registers");

            migrationBuilder.RenameTable(
                name: "Registers",
                newName: "UserContestRegistrations");

            migrationBuilder.RenameIndex(
                name: "IX_Registers_ContestId",
                table: "UserContestRegistrations",
                newName: "IX_UserContestRegistrations_ContestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserContestRegistrations",
                table: "UserContestRegistrations",
                columns: new[] { "UserId", "ContestId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserContestRegistrations_Contests_ContestId",
                table: "UserContestRegistrations",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserContestRegistrations_Contests_ContestId",
                table: "UserContestRegistrations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserContestRegistrations",
                table: "UserContestRegistrations");

            migrationBuilder.RenameTable(
                name: "UserContestRegistrations",
                newName: "Registers");

            migrationBuilder.RenameIndex(
                name: "IX_UserContestRegistrations_ContestId",
                table: "Registers",
                newName: "IX_Registers_ContestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Registers",
                table: "Registers",
                columns: new[] { "UserId", "ContestId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Registers_Contests_ContestId",
                table: "Registers",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
