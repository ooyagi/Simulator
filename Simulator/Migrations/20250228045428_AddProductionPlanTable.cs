using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simulator.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionPlanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionPlans",
                columns: table => new
                {
                    DeliveryDate = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Line = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PalletNumber = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Hinban = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPlans", x => new { x.DeliveryDate, x.Line, x.PalletNumber, x.Priority, x.Size });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlans_DeliveryDate_Line_PalletNumber_Size",
                table: "ProductionPlans",
                columns: new[] { "DeliveryDate", "Line", "PalletNumber", "Size" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionPlans");
        }
    }
}
