using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KosnicaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddApiaryToAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiaryId",
                table: "Attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_ApiaryId",
                table: "Attachments",
                column: "ApiaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Apiaries_ApiaryId",
                table: "Attachments",
                column: "ApiaryId",
                principalTable: "Apiaries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Apiaries_ApiaryId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_ApiaryId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "ApiaryId",
                table: "Attachments");
        }
    }
}
