using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistributedTransactions.Data.Migrations.AllocationsDb
{
    /// <inheritdoc />
    public partial class AllocationsDataMart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ActivityType = table.Column<int>(type: "integer", nullable: false),
                    BlockOrderCode = table.Column<string>(type: "text", nullable: false),
                    BlockOrderSeqNum = table.Column<int>(type: "integer", nullable: false),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Restriction = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: true),
                    Price = table.Column<double>(type: "double precision", nullable: true),
                    AccountNum = table.Column<string>(type: "text", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAmount = table.Column<int>(type: "integer", nullable: true),
                    NewDestination = table.Column<int>(type: "integer", nullable: true),
                    NewType = table.Column<int>(type: "integer", nullable: true),
                    NewRestriction = table.Column<int>(type: "integer", nullable: true),
                    NewAmount = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_BlockOrderCode_BlockOrderSeqNum_Date",
                table: "Trades",
                columns: new[] { "BlockOrderCode", "BlockOrderSeqNum", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
