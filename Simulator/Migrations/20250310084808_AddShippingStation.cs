using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simulator.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShippingStations",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingStations", x => x.Code);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingStations");
        }
    }
}
