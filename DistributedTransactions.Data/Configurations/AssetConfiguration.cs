using DistributedTransactions.Domain.Reference;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class AssetConfiguration : BaseEntityConfiguration<Asset>
{
    public override void Configure(EntityTypeBuilder<Asset> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => new {q.AssetClass, q.Symbol}).IsUnique();
            
        builder.HasData(
            new Asset {
                Id = new Guid("00000000-0000-0000-0001-000000000001"),
                AssetClass = "Cash",
                Symbol = "USD",
                Name = "US Dollar",
                MarketCap = null,
                Country = "United States",
                IPOYear = 1776,
                Sector = null,
                Industry = null
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000001"),
                AssetClass = "Equity",
                Symbol = "ABBV",
                Name = "AbbVie Inc. Common Stock",
                MarketCap = 299760012990.00,
                Country = "United States",
                IPOYear = 2012,
                Sector = "Health Care",
                Industry = "Biotechnology: Pharmaceutical Preparations",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000002"),
                AssetClass = "Equity",
                Symbol = "ACN",
                Name = "Accenture plc Class A Ordinary Shares (Ireland)",
                MarketCap = 226352912962.00,
                Country = "Ireland",
                IPOYear = 2001,
                Sector = "Consumer Discretionary",
                Industry = "Business Services",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000003"),
                AssetClass = "Equity",
                Symbol = "AXP",
                Name = "American Express Company Common Stock",
                MarketCap = 203091461787.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Finance",
                Industry = "Finance: Consumer Services",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000004"),
                AssetClass = "Equity",
                Symbol = "BABA",
                Name = "Alibaba Group Holding Limited American Depositary Shares each representing eight Ordinary share",
                MarketCap = 220439190005.00,
                Country = "China",
                IPOYear = 2014,
                Sector = "Consumer Discretionary",
                Industry = "Business Services",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000005"),
                AssetClass = "Equity",
                Symbol = "BAC",
                Name = "Bank of America Corporation Common Stock",
                MarketCap = 352185173594.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Finance",
                Industry = "Major Banks",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000006"),
                AssetClass = "Equity",
                Symbol = "BRKA",
                Name = "Berkshire Hathaway Inc.",
                MarketCap = 1030494147895.00,
                Country = "United States",
                IPOYear = null,
                Sector = "",
                Industry = "",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000007"),
                AssetClass = "Equity",
                Symbol = "BRKB",
                Name = "Berkshire Hathaway Inc.",
                MarketCap = 1031891828217.00,
                Country = "United States",
                IPOYear = null,
                Sector = "",
                Industry = "",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000008"),
                AssetClass = "Equity",
                Symbol = "CCZ",
                Name = "Comcast Holdings ZONES",
                MarketCap = 233036286183.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Telecommunications",
                Industry = "Cable & Other Pay Television Services",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000009"),
                AssetClass = "Equity",
                Symbol = "CRM",
                Name = "Salesforce Inc. Common Stock",
                MarketCap = 317057400000.00,
                Country = "United States",
                IPOYear = 2004,
                Sector = "Technology",
                Industry = "Computer Software: Prepackaged Software",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000010"),
                AssetClass = "Equity",
                Symbol = "CVX",
                Name = "Chevron Corporation Common Stock",
                MarketCap = 290769376385.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Energy",
                Industry = "Integrated oil Companies",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000011"),
                AssetClass = "Equity",
                Symbol = "HD",
                Name = "Home Depot Inc. (The) Common Stock",
                MarketCap = 402998988916.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Consumer Discretionary",
                Industry = "RETAIL: Building Materials",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000012"),
                AssetClass = "Equity",
                Symbol = "JNJ",
                Name = "Johnson & Johnson Common Stock",
                MarketCap = 365645700758.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Health Care",
                Industry = "Biotechnology: Pharmaceutical Preparations",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000013"),
                AssetClass = "Equity",
                Symbol = "JPM",
                Name = "JP Morgan Chase & Co. Common Stock",
                MarketCap = 680946387869.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Finance",
                Industry = "Major Banks",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000014"),
                AssetClass = "Equity",
                Symbol = "KO",
                Name = "Coca-Cola Company (The) Common Stock",
                MarketCap = 269452710982.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Consumer Staples",
                Industry = "Beverages (Production/Distribution)",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000015"),
                AssetClass = "Equity",
                Symbol = "LLY",
                Name = "Eli Lilly and Company Common Stock",
                MarketCap = 746380478094.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Health Care",
                Industry = "Biotechnology: Pharmaceutical Preparations",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000016"),
                AssetClass = "Equity",
                Symbol = "MA",
                Name = "Mastercard Incorporated Common Stock",
                MarketCap = 477639266971.00,
                Country = "United States",
                IPOYear = 2006,
                Sector = "Consumer Discretionary",
                Industry = "Business Services",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000017"),
                AssetClass = "Equity",
                Symbol = "MCD",
                Name = "McDonald's Corporation Common Stock",
                MarketCap = 213953973452.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Consumer Discretionary",
                Industry = "Restaurants",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000018"),
                AssetClass = "Equity",
                Symbol = "MRK",
                Name = "Merck & Company Inc. Common Stock (new)",
                MarketCap = 248814962042.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Health Care",
                Industry = "Biotechnology: Pharmaceutical Preparations",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000019"),
                AssetClass = "Equity",
                Symbol = "MS",
                Name = "Morgan Stanley Common Stock",
                MarketCap = 213349441727.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Finance",
                Industry = "Investment Bankers/Brokers/Service",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000020"),
                AssetClass = "Equity",
                Symbol = "NOW",
                Name = "ServiceNow Inc. Common Stock",
                MarketCap = 214198800000.00,
                Country = "United States",
                IPOYear = 2012,
                Sector = "Technology",
                Industry = "Computer Software: Prepackaged Software",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000021"),
                AssetClass = "Equity",
                Symbol = "NVO",
                Name = "Novo Nordisk A/S Common Stock",
                MarketCap = 469961989139.00,
                Country = "Denmark",
                IPOYear = null,
                Sector = "Health Care",
                Industry = "Biotechnology: Pharmaceutical Preparations",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000022"),
                AssetClass = "Equity",
                Symbol = "NVS",
                Name = "Novartis AG Common Stock",
                MarketCap = 211353114152.00,
                Country = "Switzerland",
                IPOYear = null,
                Sector = "Health Care",
                Industry = "Biotechnology: Pharmaceutical Preparations",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000023"),
                AssetClass = "Equity",
                Symbol = "ORCL",
                Name = "Oracle Corporation Common Stock",
                MarketCap = 518309564400.00,
                Country = "United States",
                IPOYear = 1986,
                Sector = "Technology",
                Industry = "Computer Software: Prepackaged Software",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000024"),
                AssetClass = "Equity",
                Symbol = "PG",
                Name = "Procter & Gamble Company (The) Common Stock",
                MarketCap = 393480372081.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Consumer Discretionary",
                Industry = "Package Goods/Cosmetics",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000025"),
                AssetClass = "Equity",
                Symbol = "SAP",
                Name = "SAP  SE ADS",
                MarketCap = 270096803558.00,
                Country = "Germany",
                IPOYear = null,
                Sector = "Technology",
                Industry = "Computer Software: Prepackaged Software",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000026"),
                AssetClass = "Equity",
                Symbol = "SHEL",
                Name = "Shell PLC American Depositary Shares (each representing two (2) Ordinary Shares)",
                MarketCap = 204747652705.00,
                Country = "Netherlands",
                IPOYear = 2022,
                Sector = "Energy",
                Industry = "Oil & Gas Production",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000027"),
                AssetClass = "Equity",
                Symbol = "SMFG",
                Name = "Sumitomo Mitsui Financial Group Inc Unsponsored American Depositary Shares (Japan)",
                MarketCap = 280448751621.00,
                Country = "Japan",
                IPOYear = null,
                Sector = "Finance",
                Industry = "Commercial Banks",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000028"),
                AssetClass = "Equity",
                Symbol = "TM",
                Name = "Toyota Motor Corporation Common Stock",
                MarketCap = 233749936376.00,
                Country = "Japan",
                IPOYear = null,
                Sector = "Consumer Discretionary",
                Industry = "Auto Manufacturing",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000029"),
                AssetClass = "Equity",
                Symbol = "TMO",
                Name = "Thermo Fisher Scientific Inc Common Stock",
                MarketCap = 203880291250.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Industrials",
                Industry = "Industrial Machinery/Components",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000030"),
                AssetClass = "Equity",
                Symbol = "TSM",
                Name = "Taiwan Semiconductor Manufacturing Company Ltd.",
                MarketCap = 977639076323.00,
                Country = "Taiwan",
                IPOYear = 1997,
                Sector = "Technology",
                Industry = "Semiconductors",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000031"),
                AssetClass = "Equity",
                Symbol = "UNH",
                Name = "UnitedHealth Group Incorporated Common Stock (DE)",
                MarketCap = 545866652712.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Health Care",
                Industry = "Medical Specialities",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000032"),
                AssetClass = "Equity",
                Symbol = "V",
                Name = "Visa Inc.",
                MarketCap = 561725893240.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Consumer Discretionary",
                Industry = "Business Services",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000033"),
                AssetClass = "Equity",
                Symbol = "WFC",
                Name = "Wells Fargo & Company Common Stock",
                MarketCap = 242386919975.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Finance",
                Industry = "Major Banks",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000034"),
                AssetClass = "Equity",
                Symbol = "WMT",
                Name = "Walmart Inc. Common Stock",
                MarketCap = 678991076668.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Consumer Discretionary",
                Industry = "Department/Specialty Retail Stores",
            },
            new Asset {
                Id = new Guid("00000000-0000-0000-0000-000000000035"),
                AssetClass = "Equity",
                Symbol = "XOM",
                Name = "Exxon Mobil Corporation Common Stock",
                MarketCap = 529872597260.00,
                Country = "United States",
                IPOYear = null,
                Sector = "Energy",
                Industry = "Integrated oil Companies",
            }
        );
    }
}