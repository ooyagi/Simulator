using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simulator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedColums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "ShikakariStorages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "ShikakariStorages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
