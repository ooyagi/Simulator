using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simulator.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityToWorkOrderAndShippingPalllet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ShippingPallets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ShippingPallets");
        }
    }
}
