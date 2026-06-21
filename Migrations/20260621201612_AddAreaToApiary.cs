using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KosnicaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaToApiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Area",
                table: "Apiaries",
                type: "integer",
                nullable: false,
                defaultValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Apiaries");
        }
    }
}
