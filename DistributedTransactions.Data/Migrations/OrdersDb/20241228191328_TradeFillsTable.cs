using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistributedTransactions.Data.Migrations.OrdersDb
{
    /// <inheritdoc />
    public partial class TradeFillsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradeFills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    BlockOrderCode = table.Column<string>(type: "text", nullable: false),
                    BlockOrderSeqNum = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FilledQuantity = table.Column<long>(type: "bigint", nullable: true),
                    Price = table.Column<double>(type: "numeric(17,2)", nullable: true),
                    CancelledQuantity = table.Column<long>(type: "bigint", nullable: true),
                    NewDestination = table.Column<int>(type: "integer", nullable: true),
                    NewType = table.Column<int>(type: "integer", nullable: true),
                    NewRestriction = table.Column<int>(type: "integer", nullable: true),
                    NewQuantity = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeFills", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TradeFills_BlockOrderCode_BlockOrderSeqNum_Date",
                table: "TradeFills",
                columns: new[] { "BlockOrderCode", "BlockOrderSeqNum", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TradeFills");
        }
    }
}
