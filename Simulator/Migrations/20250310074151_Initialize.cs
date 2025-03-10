using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simulator.Migrations
{
    /// <inheritdoc />
    public partial class Initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryPallets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Hinban = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryPallets", x => x.Id);
                });

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
                    table.PrimaryKey("PK_ProductionPlans", x => new { x.DeliveryDate, x.Line, x.Size, x.PalletNumber, x.Priority });
                });

            migrationBuilder.CreateTable(
                name: "ShippingPallets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingPallets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransportRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    PalletID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeliveryDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Line = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PalletNumber = table.Column<int>(type: "int", nullable: false),
                    Assigned = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.PalletID);
                });

            migrationBuilder.CreateTable(
                name: "InventoryStorages",
                columns: table => new
                {
                    LocationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InventoryPalletID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryStorages", x => x.LocationCode);
                    table.ForeignKey(
                        name: "FK_InventoryStorages_InventoryPallets_InventoryPalletID",
                        column: x => x.InventoryPalletID,
                        principalTable: "InventoryPallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TemporaryStorages",
                columns: table => new
                {
                    LocationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShippingStationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InventoryPalletID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryStorages", x => x.LocationCode);
                    table.ForeignKey(
                        name: "FK_TemporaryStorages_InventoryPallets_InventoryPalletID",
                        column: x => x.InventoryPalletID,
                        principalTable: "InventoryPallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShikakariStorages",
                columns: table => new
                {
                    LocationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ShippingPalletID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShikakariStorages", x => x.LocationCode);
                    table.ForeignKey(
                        name: "FK_ShikakariStorages_ShippingPallets_ShippingPalletID",
                        column: x => x.ShippingPalletID,
                        principalTable: "ShippingPallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShippingStorages",
                columns: table => new
                {
                    LocationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShippingStationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ShippingPalletID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingStorages", x => x.LocationCode);
                    table.ForeignKey(
                        name: "FK_ShippingStorages_ShippingPallets_ShippingPalletID",
                        column: x => x.ShippingPalletID,
                        principalTable: "ShippingPallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderedItems",
                columns: table => new
                {
                    PalletID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Hinban = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderedItems", x => new { x.PalletID, x.Index });
                    table.ForeignKey(
                        name: "FK_OrderedItems_WorkOrders_PalletID",
                        column: x => x.PalletID,
                        principalTable: "WorkOrders",
                        principalColumn: "PalletID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStorages_InventoryPalletID",
                table: "InventoryStorages",
                column: "InventoryPalletID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlans_DeliveryDate_Line_Size_PalletNumber",
                table: "ProductionPlans",
                columns: new[] { "DeliveryDate", "Line", "Size", "PalletNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ShikakariStorages_ShippingPalletID",
                table: "ShikakariStorages",
                column: "ShippingPalletID");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingStorages_ShippingPalletID",
                table: "ShippingStorages",
                column: "ShippingPalletID");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryStorages_InventoryPalletID",
                table: "TemporaryStorages",
                column: "InventoryPalletID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryStorages");

            migrationBuilder.DropTable(
                name: "OrderedItems");

            migrationBuilder.DropTable(
                name: "ProductionPlans");

            migrationBuilder.DropTable(
                name: "ShikakariStorages");

            migrationBuilder.DropTable(
                name: "ShippingStorages");

            migrationBuilder.DropTable(
                name: "TemporaryStorages");

            migrationBuilder.DropTable(
                name: "TransportRecords");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "ShippingPallets");

            migrationBuilder.DropTable(
                name: "InventoryPallets");
        }
    }
}
