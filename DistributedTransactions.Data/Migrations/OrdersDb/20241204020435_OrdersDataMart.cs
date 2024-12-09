using System;
using System.Collections.Generic;
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
                    RequestNumber = table.Column<int>(type: "integer", nullable: false),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Restriction = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Filled = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Cancelled = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Needed = table.Column<int>(type: "integer", nullable: false, computedColumnSql: "\"Quantity\" - \"Filled\" - \"Cancelled\"", stored: true),
                    Price = table.Column<double>(type: "numeric(17,2)", nullable: true),
                    Amount = table.Column<double>(type: "numeric(17,2)", nullable: true, computedColumnSql: "\"Filled\" * \"Price\"", stored: true)
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
                    RequestNumber = table.Column<int>(type: "integer", nullable: false),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Restriction = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Filled = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Cancelled = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Needed = table.Column<int>(type: "integer", nullable: false, computedColumnSql: "\"Quantity\" - \"Filled\" - \"Cancelled\"", stored: true),
                    Price = table.Column<double>(type: "numeric(17,2)", nullable: true),
                    Amount = table.Column<double>(type: "numeric(17,2)", nullable: true, computedColumnSql: "\"Filled\" * \"Price\"", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RebalancingGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    GroupNumber = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "unordered_unique_rowid()"),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    ManagerName = table.Column<string>(type: "text", nullable: false),
                    Strategy = table.Column<int>(type: "integer", nullable: false),
                    AccountNumbers = table.Column<List<string>>(type: "text[]", nullable: false),
                    SecuritySymbols = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RebalancingGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RebalancingRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    RequestNumber = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "unordered_unique_rowid()"),
                    GroupNumber = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RebalancingRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RebalancingSecurities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    RequestNumber = table.Column<int>(type: "integer", nullable: false),
                    GroupNumber = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccountNumbers = table.Column<List<string>>(type: "text[]", nullable: false),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RebalancingSecurities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockOrders_Code_Date",
                table: "BlockOrders",
                columns: new[] { "Code", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlockOrders_RequestNumber_Symbol_Direction",
                table: "BlockOrders",
                columns: new[] { "RequestNumber", "Symbol", "Direction" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_RequestNumber_PositionId",
                table: "CustomerOrders",
                columns: new[] { "RequestNumber", "PositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RebalancingGroups_AssetClass_ManagerName_Strategy",
                table: "RebalancingGroups",
                columns: new[] { "AssetClass", "ManagerName", "Strategy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RebalancingGroups_GroupNumber",
                table: "RebalancingGroups",
                column: "GroupNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RebalancingRequests_GroupNumber_Date",
                table: "RebalancingRequests",
                columns: new[] { "GroupNumber", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RebalancingRequests_RequestNumber",
                table: "RebalancingRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RebalancingSecurities_RequestNumber_Symbol",
                table: "RebalancingSecurities",
                columns: new[] { "RequestNumber", "Symbol" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockOrders");

            migrationBuilder.DropTable(
                name: "CustomerOrders");

            migrationBuilder.DropTable(
                name: "RebalancingGroups");

            migrationBuilder.DropTable(
                name: "RebalancingRequests");

            migrationBuilder.DropTable(
                name: "RebalancingSecurities");
        }
    }
}
