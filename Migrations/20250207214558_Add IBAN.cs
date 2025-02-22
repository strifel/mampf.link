using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupOrder.Migrations
{
    /// <inheritdoc />
    public partial class AddIBAN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "MenuURL", table: "Groups", newName: "MenuUrl");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Groups",
                type: "TEXT",
                maxLength: 100,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Groups",
                type: "TEXT",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "BankName", table: "Groups");

            migrationBuilder.DropColumn(name: "IBAN", table: "Groups");

            migrationBuilder.RenameColumn(name: "MenuUrl", table: "Groups", newName: "MenuURL");
        }
    }
}
