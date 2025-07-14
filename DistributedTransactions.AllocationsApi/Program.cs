using Npgsql;
using DistributedTransactions.AllocationsApi.Models;
using DistributedTransactions.AllocationsApi.Utils;
using DistributedTransactions.Domain.Allocations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Output to console
builder.Logging.AddDebug();   // Output to Debug window (e.g., in VS)

builder.Services.AddLogging(logging => {
    logging.AddFilter("Npgsql", LogLevel.Debug); // Capture Npgsql logs only
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(7200);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/allocations/search", async(
    IConfiguration config,
    DateTime date,
    string? symbol,
    TradeDirection? direction,
    TradeType? type,
    TradeDestination? destination,
    int pageSize = 100
) => {
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? config.GetConnectionString("AllocationsDb")
        ?? throw new InvalidOperationException("No database connection string found.");
    var repo = new TradeRepository(connectionString);
    TradeSearchCursor activeCursor;
    List<TradeSearchPage> allPages = [];

    // Get asOfSystemTime and total number of search results
    var asOf = await repo.GetFollowerReadTimestampAsync();
    var totalRecords = await repo.GetTotalCountAsync(asOf, date, symbol, direction, type, destination);

    // Get all page-start cursors
    allPages = await repo.GetPageCursorsAsync(asOf, date, pageSize, symbol, direction, type, destination);

    if (!allPages.Any())
    {
        return Results.Ok(new PagedResponse<Trade>
        {
            Data = Enumerable.Empty<Trade>(),
            Page = 0,
            Offset = 0,
            Count = 0,
            AsOfSystemTime = asOf,
            Date = date,
            PageSize = pageSize,
            SearchSymbol = symbol,
            Direction = direction,
            Type = type,
            Destination = destination,
            TotalRecords = 0,
            TotalPages = 0,
            Pages = []
        });
    }

    // Use the first page’s cursor
    var firstCursor = allPages.First().Cursor;
    if (firstCursor is null)
        return Results.Problem("First page cursor is null.");
    activeCursor = CursorUtils.Decode(firstCursor);

    // Now fetch the current page
    var trades = await repo.GetTradesPageAsync(activeCursor);

    return Results.Ok(new PagedResponse<Trade>
    {
        Data = trades,
        Page = activeCursor.Page,
        Offset = activeCursor.Offset,
        Count = trades.Count,
        AsOfSystemTime = activeCursor.AsOfSystemTime,
        Date = activeCursor.Date,
        PageSize = activeCursor.PageSize,
        SearchSymbol = activeCursor.SearchSymbol,
        Direction = activeCursor.Direction,
        Type = activeCursor.Type,
        Destination = activeCursor.Destination,
        TotalRecords = totalRecords,
        TotalPages = allPages.Count,
        Pages = allPages
    });
})
.WithName("SearchAllocations")
.WithOpenApi();

app.MapGet("/allocations/page", async(
    IConfiguration config,
    string cursor
) => {
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? config.GetConnectionString("AllocationsDb")
        ?? throw new InvalidOperationException("No database connection string found.");
    var repo = new TradeRepository(connectionString);
    TradeSearchCursor activeCursor = CursorUtils.Decode(cursor);

    // Now fetch the page using the activeCursor
    var trades = await repo.GetTradesPageAsync(activeCursor);

    return Results.Ok(new PagedResponse<Trade>
    {
        Data = trades,
        Page = activeCursor.Page,
        Offset = activeCursor.Offset,
        Count = trades.Count,
        AsOfSystemTime = activeCursor.AsOfSystemTime,
        Date = activeCursor.Date,
        PageSize = activeCursor.PageSize,
        SearchSymbol = activeCursor.SearchSymbol,
        Direction = activeCursor.Direction,
        Type = activeCursor.Type,
        Destination = activeCursor.Destination
    });
})
.WithName("GetAllocations")
.WithOpenApi();

app.Run();
