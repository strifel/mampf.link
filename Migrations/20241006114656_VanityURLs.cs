using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupOrder.Migrations
{
    /// <inheritdoc />
    public partial class VanityURLs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VanityURLId",
                table: "Groups",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VanityUrls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VanityName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AdminCode = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    UsedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanityUrls", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_VanityURLId",
                table: "Groups",
                column: "VanityURLId");

            migrationBuilder.CreateIndex(
                name: "IX_VanityUrls_Slug",
                table: "VanityUrls",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_VanityUrls_VanityURLId",
                table: "Groups",
                column: "VanityURLId",
                principalTable: "VanityUrls",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_VanityUrls_VanityURLId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "VanityUrls");

            migrationBuilder.DropIndex(
                name: "IX_Groups_VanityURLId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "VanityURLId",
                table: "Groups");
        }
    }
}
