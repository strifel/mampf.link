using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupOrder.Migrations
{
    /// <inheritdoc />
    public partial class Orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Order_Groups_GroupId", table: "Order");

            migrationBuilder.DropPrimaryKey(name: "PK_Order", table: "Order");

            migrationBuilder.RenameTable(name: "Order", newName: "Orders");

            migrationBuilder.RenameIndex(
                name: "IX_Order_GroupId",
                table: "Orders",
                newName: "IX_Orders_GroupId"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Orders", table: "Orders", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Groups_GroupId",
                table: "Orders",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Orders_Groups_GroupId", table: "Orders");

            migrationBuilder.DropPrimaryKey(name: "PK_Orders", table: "Orders");

            migrationBuilder.RenameTable(name: "Orders", newName: "Order");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_GroupId",
                table: "Order",
                newName: "IX_Order_GroupId"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Order", table: "Order", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Groups_GroupId",
                table: "Order",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
