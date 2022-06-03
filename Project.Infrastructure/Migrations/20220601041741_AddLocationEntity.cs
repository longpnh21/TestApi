using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Project.Infrastructure.Migrations
{
    public partial class AddLocationEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "LostProperties");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "LostProperties",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Floor = table.Column<int>(type: "integer", nullable: true),
                    Cube = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LostProperties_LocationId",
                table: "LostProperties",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LostProperties_Locations_LocationId",
                table: "LostProperties",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LostProperties_Locations_LocationId",
                table: "LostProperties");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_LostProperties_LocationId",
                table: "LostProperties");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "LostProperties");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "LostProperties",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
