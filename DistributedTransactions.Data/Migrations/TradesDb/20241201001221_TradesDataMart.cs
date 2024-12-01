using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistributedTransactions.Data.Migrations.TradesDb
{
    /// <inheritdoc />
    public partial class TradesDataMart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BustedTrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CancelledQuantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ActivityType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BlockOrderCode = table.Column<string>(type: "text", nullable: false),
                    BlockOrderSeqNum = table.Column<int>(type: "integer", nullable: false),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Restriction = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Price = table.Column<double>(type: "numeric(17,2)", nullable: true),
                    Amount = table.Column<double>(type: "numeric(17,2)", nullable: true, computedColumnSql: "\"Quantity\" * \"Price\"", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BustedTrades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExecutedTrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ActivityType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    BlockOrderCode = table.Column<string>(type: "text", nullable: false),
                    BlockOrderSeqNum = table.Column<int>(type: "integer", nullable: false),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Restriction = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Price = table.Column<double>(type: "numeric(17,2)", nullable: true),
                    Amount = table.Column<double>(type: "numeric(17,2)", nullable: true, computedColumnSql: "\"Quantity\" * \"Price\"", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutedTrades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReplacedTrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    NewDestination = table.Column<int>(type: "integer", nullable: true),
                    NewType = table.Column<int>(type: "integer", nullable: true),
                    NewRestriction = table.Column<int>(type: "integer", nullable: true),
                    NewQuantity = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ActivityType = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    BlockOrderCode = table.Column<string>(type: "text", nullable: false),
                    BlockOrderSeqNum = table.Column<int>(type: "integer", nullable: false),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Restriction = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Price = table.Column<double>(type: "numeric(17,2)", nullable: true),
                    Amount = table.Column<double>(type: "numeric(17,2)", nullable: true, computedColumnSql: "\"Quantity\" * \"Price\"", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplacedTrades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BustedTrades_BlockOrderCode_BlockOrderSeqNum_Date",
                table: "BustedTrades",
                columns: new[] { "BlockOrderCode", "BlockOrderSeqNum", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExecutedTrades_BlockOrderCode_BlockOrderSeqNum_Date",
                table: "ExecutedTrades",
                columns: new[] { "BlockOrderCode", "BlockOrderSeqNum", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReplacedTrades_BlockOrderCode_BlockOrderSeqNum_Date",
                table: "ReplacedTrades",
                columns: new[] { "BlockOrderCode", "BlockOrderSeqNum", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BustedTrades");

            migrationBuilder.DropTable(
                name: "ExecutedTrades");

            migrationBuilder.DropTable(
                name: "ReplacedTrades");
        }
    }
}
