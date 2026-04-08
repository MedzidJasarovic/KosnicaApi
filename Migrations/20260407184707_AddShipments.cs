using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KosnicaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddShipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_Hives_HiveId",
                table: "Interventions");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldRecords_Hives_HiveId",
                table: "YieldRecords");

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProductType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceiverName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_UserId",
                table: "Shipments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_Hives_HiveId",
                table: "Interventions",
                column: "HiveId",
                principalTable: "Hives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YieldRecords_Hives_HiveId",
                table: "YieldRecords",
                column: "HiveId",
                principalTable: "Hives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interventions_Hives_HiveId",
                table: "Interventions");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldRecords_Hives_HiveId",
                table: "YieldRecords");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.AddForeignKey(
                name: "FK_Interventions_Hives_HiveId",
                table: "Interventions",
                column: "HiveId",
                principalTable: "Hives",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YieldRecords_Hives_HiveId",
                table: "YieldRecords",
                column: "HiveId",
                principalTable: "Hives",
                principalColumn: "Id");
        }
    }
}
