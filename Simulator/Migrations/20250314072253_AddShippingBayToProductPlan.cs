using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simulator.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingBayToProductPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingBay",
                table: "ProductionPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingBay",
                table: "ProductionPlans");
        }
    }
}
