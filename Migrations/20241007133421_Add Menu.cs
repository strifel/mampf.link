using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupOrder.Migrations
{
    /// <inheritdoc />
    public partial class AddMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MenuURL",
                table: "Groups",
                type: "TEXT",
                maxLength: 100,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "MenuURL", table: "Groups");
        }
    }
}
