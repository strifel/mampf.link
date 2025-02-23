using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupOrder.Migrations
{
    /// <inheritdoc />
    public partial class AddPayBeforeClosingTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PayAfterClosingTime",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PayAfterClosingTime", table: "Groups");
        }
    }
}
