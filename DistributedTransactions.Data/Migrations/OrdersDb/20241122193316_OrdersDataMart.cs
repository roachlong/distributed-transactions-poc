using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistributedTransactions.Data.Migrations.OrdersDb
{
    /// <inheritdoc />
    public partial class OrdersDataMart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Allocated = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Accounts = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Restriction = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Filled = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Needed = table.Column<int>(type: "integer", nullable: false, computedColumnSql: "\"Amount\" - \"Filled\"", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AccountNum = table.Column<string>(type: "text", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Restriction = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Filled = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Needed = table.Column<int>(type: "integer", nullable: false, computedColumnSql: "\"Amount\" - \"Filled\"", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockOrders_AssetClass_Symbol_Date_Direction_Destination_Ty~",
                table: "BlockOrders",
                columns: new[] { "AssetClass", "Symbol", "Date", "Direction", "Destination", "Type", "Restriction" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlockOrders_Code_Date",
                table: "BlockOrders",
                columns: new[] { "Code", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_PositionId_AssetClass_Symbol_Date",
                table: "CustomerOrders",
                columns: new[] { "PositionId", "AssetClass", "Symbol", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockOrders");

            migrationBuilder.DropTable(
                name: "CustomerOrders");
        }
    }
}
