using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KosnicaApi.Migrations
{
    /// <inheritdoc />
    public partial class YieldsAndTeamRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployerId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Interventions");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Interventions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployerId",
                table: "Users",
                column: "EmployerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_EmployerId",
                table: "Users",
                column: "EmployerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_EmployerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmployerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmployerId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Interventions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
