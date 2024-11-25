using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DistributedTransactions.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AssetClass = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MarketCap = table.Column<double>(type: "double precision", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    IPOYear = table.Column<int>(type: "integer", nullable: true),
                    Sector = table.Column<string>(type: "text", nullable: true),
                    Industry = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system"),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false, defaultValue: "system")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "AssetClass", "Country", "IPOYear", "Industry", "MarketCap", "Name", "Sector", "Symbol" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Equity", "United States", 2012, "Biotechnology: Pharmaceutical Preparations", 299760012990.0, "AbbVie Inc. Common Stock", "Health Care", "ABBV" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Equity", "Ireland", 2001, "Business Services", 226352912962.0, "Accenture plc Class A Ordinary Shares (Ireland)", "Consumer Discretionary", "ACN" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Equity", "United States", null, "Finance: Consumer Services", 203091461787.0, "American Express Company Common Stock", "Finance", "AXP" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Equity", "China", 2014, "Business Services", 220439190005.0, "Alibaba Group Holding Limited American Depositary Shares each representing eight Ordinary share", "Consumer Discretionary", "BABA" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Equity", "United States", null, "Major Banks", 352185173594.0, "Bank of America Corporation Common Stock", "Finance", "BAC" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "Equity", "United States", null, "", 1030494147895.0, "Berkshire Hathaway Inc.", "", "BRKA" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "Equity", "United States", null, "", 1031891828217.0, "Berkshire Hathaway Inc.", "", "BRKB" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "Equity", "United States", null, "Cable & Other Pay Television Services", 233036286183.0, "Comcast Holdings ZONES", "Telecommunications", "CCZ" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "Equity", "United States", 2004, "Computer Software: Prepackaged Software", 317057400000.0, "Salesforce Inc. Common Stock", "Technology", "CRM" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "Equity", "United States", null, "Integrated oil Companies", 290769376385.0, "Chevron Corporation Common Stock", "Energy", "CVX" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "Equity", "United States", null, "RETAIL: Building Materials", 402998988916.0, "Home Depot Inc. (The) Common Stock", "Consumer Discretionary", "HD" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "Equity", "United States", null, "Biotechnology: Pharmaceutical Preparations", 365645700758.0, "Johnson & Johnson Common Stock", "Health Care", "JNJ" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "Equity", "United States", null, "Major Banks", 680946387869.0, "JP Morgan Chase & Co. Common Stock", "Finance", "JPM" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "Equity", "United States", null, "Beverages (Production/Distribution)", 269452710982.0, "Coca-Cola Company (The) Common Stock", "Consumer Staples", "KO" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "Equity", "United States", null, "Biotechnology: Pharmaceutical Preparations", 746380478094.0, "Eli Lilly and Company Common Stock", "Health Care", "LLY" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "Equity", "United States", 2006, "Business Services", 477639266971.0, "Mastercard Incorporated Common Stock", "Consumer Discretionary", "MA" },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "Equity", "United States", null, "Restaurants", 213953973452.0, "McDonald's Corporation Common Stock", "Consumer Discretionary", "MCD" },
                    { new Guid("00000000-0000-0000-0000-000000000018"), "Equity", "United States", null, "Biotechnology: Pharmaceutical Preparations", 248814962042.0, "Merck & Company Inc. Common Stock (new)", "Health Care", "MRK" },
                    { new Guid("00000000-0000-0000-0000-000000000019"), "Equity", "United States", null, "Investment Bankers/Brokers/Service", 213349441727.0, "Morgan Stanley Common Stock", "Finance", "MS" },
                    { new Guid("00000000-0000-0000-0000-000000000020"), "Equity", "United States", 2012, "Computer Software: Prepackaged Software", 214198800000.0, "ServiceNow Inc. Common Stock", "Technology", "NOW" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "Equity", "Denmark", null, "Biotechnology: Pharmaceutical Preparations", 469961989139.0, "Novo Nordisk A/S Common Stock", "Health Care", "NVO" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "Equity", "Switzerland", null, "Biotechnology: Pharmaceutical Preparations", 211353114152.0, "Novartis AG Common Stock", "Health Care", "NVS" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "Equity", "United States", 1986, "Computer Software: Prepackaged Software", 518309564400.0, "Oracle Corporation Common Stock", "Technology", "ORCL" },
                    { new Guid("00000000-0000-0000-0000-000000000024"), "Equity", "United States", null, "Package Goods/Cosmetics", 393480372081.0, "Procter & Gamble Company (The) Common Stock", "Consumer Discretionary", "PG" },
                    { new Guid("00000000-0000-0000-0000-000000000025"), "Equity", "Germany", null, "Computer Software: Prepackaged Software", 270096803558.0, "SAP  SE ADS", "Technology", "SAP" },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "Equity", "Netherlands", 2022, "Oil & Gas Production", 204747652705.0, "Shell PLC American Depositary Shares (each representing two (2) Ordinary Shares)", "Energy", "SHEL" },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "Equity", "Japan", null, "Commercial Banks", 280448751621.0, "Sumitomo Mitsui Financial Group Inc Unsponsored American Depositary Shares (Japan)", "Finance", "SMFG" },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "Equity", "Japan", null, "Auto Manufacturing", 233749936376.0, "Toyota Motor Corporation Common Stock", "Consumer Discretionary", "TM" },
                    { new Guid("00000000-0000-0000-0000-000000000029"), "Equity", "United States", null, "Industrial Machinery/Components", 203880291250.0, "Thermo Fisher Scientific Inc Common Stock", "Industrials", "TMO" },
                    { new Guid("00000000-0000-0000-0000-000000000030"), "Equity", "Taiwan", 1997, "Semiconductors", 977639076323.0, "Taiwan Semiconductor Manufacturing Company Ltd.", "Technology", "TSM" },
                    { new Guid("00000000-0000-0000-0000-000000000031"), "Equity", "United States", null, "Medical Specialities", 545866652712.0, "UnitedHealth Group Incorporated Common Stock (DE)", "Health Care", "UNH" },
                    { new Guid("00000000-0000-0000-0000-000000000032"), "Equity", "United States", null, "Business Services", 561725893240.0, "Visa Inc.", "Consumer Discretionary", "V" },
                    { new Guid("00000000-0000-0000-0000-000000000033"), "Equity", "United States", null, "Major Banks", 242386919975.0, "Wells Fargo & Company Common Stock", "Finance", "WFC" },
                    { new Guid("00000000-0000-0000-0000-000000000034"), "Equity", "United States", null, "Department/Specialty Retail Stores", 678991076668.0, "Walmart Inc. Common Stock", "Consumer Discretionary", "WMT" },
                    { new Guid("00000000-0000-0000-0000-000000000035"), "Equity", "United States", null, "Integrated oil Companies", 529872597260.0, "Exxon Mobil Corporation Common Stock", "Energy", "XOM" },
                    { new Guid("00000000-0000-0000-0001-000000000001"), "Cash", "United States", 1776, null, null, "US Dollar", null, "USD" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetClass_Symbol",
                table: "Assets",
                columns: new[] { "AssetClass", "Symbol" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
