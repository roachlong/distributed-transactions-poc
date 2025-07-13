using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DistributedTransactions.TradeViewer
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient = new();
        private List<TradeSearchPage> _pages = new();
        private readonly Dictionary<int, List<Trade>> _pageCache = new();
        private List<Trade> _currentPageRecords = new();
        private int _localPageIndex = 0;
        private const int RecordsPerView = 10;

        private string? _selectedDirection;
        private string? _selectedType;
        private string? _selectedDestination;

        public List<string> DirectionOptions { get; } = new() { "Hold", "Buy", "Sell" };
        public List<string> TypeOptions { get; } = new() { "Limit", "Market", "Stop", "StopLimit", "TrailingStop" };
        public List<string> DestinationOptions { get; } = new()
        {
            "BSE", "Cboe", "CBOT", "CME", "CHX", "ISE", "MS4X", "Nasdaq", "NSX", "NYSE", "PHLX"
        };

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private async void OnDirectionPickerClicked(object sender, EventArgs e)
        {
            var choice = await DisplayActionSheet("Select Direction", "Cancel", null, DirectionOptions.ToArray());
            if (!string.IsNullOrEmpty(choice) && choice != "Cancel")
            {
                _selectedDirection = choice;
                DirectionPickerBtn.Text = choice;
            }
        }

        private async void OnTypePickerClicked(object sender, EventArgs e)
        {
            var choice = await DisplayActionSheet("Select Type", "Cancel", null, TypeOptions.ToArray());
            if (!string.IsNullOrEmpty(choice) && choice != "Cancel")
            {
                _selectedType = choice;
                TypePickerBtn.Text = choice;
            }
        }

        private async void OnDestinationPickerClicked(object sender, EventArgs e)
        {
            var choice = await DisplayActionSheet("Select Destination", "Cancel", null, DestinationOptions.ToArray());
            if (!string.IsNullOrEmpty(choice) && choice != "Cancel")
            {
                _selectedDestination = choice;
                DestinationPickerBtn.Text = choice;
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            TradeCollection.ItemsSource = null;
            PageButtonsPanel.Children.Clear();
            _pageCache.Clear();
            _currentPageRecords.Clear();
            _localPageIndex = 0;

            var date = DatePicker.Date;
            var symbol = SymbolEntry.Text;

            string? direction = null, type = null, dest = null;
            if (!string.IsNullOrEmpty(_selectedDirection))
                direction = ((int)Enum.Parse<TradeDirection>(_selectedDirection)).ToString();
            if (!string.IsNullOrEmpty(_selectedType))
                type = ((int)Enum.Parse<TradeType>(_selectedType)).ToString();
            if (!string.IsNullOrEmpty(_selectedDestination))
                dest = ((int)Enum.Parse<TradeDestination>(_selectedDestination)).ToString();

            var pageSize = int.TryParse(PageSizeEntry.Text, out var ps) ? ps : 100;
            var preload = int.TryParse(PreloadEntry.Text, out var pl) ? pl : 1;

            var url = $"http://localhost:7200/allocations/search?date={date:yyyy-MM-dd}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(symbol)) url += $"&symbol={symbol}";
            if (direction != null) url += $"&direction={direction}";
            if (type != null) url += $"&type={type}";
            if (dest != null) url += $"&destination={dest}";

            try
            {
                var appSw = Stopwatch.StartNew();
                var sw = Stopwatch.StartNew();
                var response = await _httpClient.GetFromJsonAsync<PagedResponse<Trade>>(url);
                sw.Stop();
                var initialSearchTime = sw.ElapsedMilliseconds;

                if (response?.Pages?.Count > 0)
                {
                    _pages = response.Pages;
                    _pageCache[1] = response.Data;
                    _currentPageRecords = response.Data;
                    UpdateLocalPage(0);

                    await DisplayAlert("Search Results",
                        $"Total Pages: {_pages.Count}\nInitial search time: {initialSearchTime} ms",
                        "OK");

                    var preloadTasks = new List<Task<(int Page, List<Trade>? Data)>>();
                    foreach (var p in _pages)
                    {
                        if (p.Page > 1 && p.Page <= preload)
                        {
                            preloadTasks.Add(Task.Run(async () =>
                            {
                                var pageData = await FetchPage(p.Cursor);
                                return (p.Page, pageData?.Data);
                            }));
                        }
                    }

                    sw.Restart();
                    var preloadResults = await Task.WhenAll(preloadTasks);
                    sw.Stop();
                    var preloadTime = sw.ElapsedMilliseconds;

                    int addedToCache = 0;
                    foreach (var (page, data) in preloadResults)
                    {
                        if (data != null)
                        {
                            _pageCache[page] = data;
                            addedToCache++;
                        }
                    }

                    appSw.Stop();
                    var appTime = appSw.ElapsedMilliseconds - initialSearchTime - preloadTime;

                    await DisplayAlert("Preload Summary",
                        $"Preloaded Pages: {preload}\nPreload time: {preloadTime} ms\nAdded to Cache: {addedToCache}\nApp time (post-load): {appTime} ms",
                        "OK");

                    foreach (var p in _pages)
                    {
                        var btn = new Button { Text = $"Page {p.Page}" };
                        btn.Clicked += async (_, _) =>
                        {
                            if (_pageCache.TryGetValue(p.Page, out var cached))
                            {
                                _currentPageRecords = cached;
                                UpdateLocalPage(0);
                                await DisplayAlert("Page Load",
                                    $"Page {p.Page} loaded from cache.\nRecords: {cached.Count}",
                                    "OK");
                            }
                            else
                            {
                                var pageSw = Stopwatch.StartNew();
                                var appLoadSw = Stopwatch.StartNew();
                                var page = await FetchPage(p.Cursor);
                                pageSw.Stop();

                                if (page?.Data != null)
                                {
                                    _pageCache[p.Page] = page.Data;
                                    _currentPageRecords = page.Data;
                                    UpdateLocalPage(0);
                                    appLoadSw.Stop();
                                    await DisplayAlert("Page Load",
                                        $"Page {p.Page} fetched from API in {pageSw.ElapsedMilliseconds} ms\nApp time: {appLoadSw.ElapsedMilliseconds - pageSw.ElapsedMilliseconds} ms\nRecords: {page.Data.Count}",
                                        "OK");
                                }
                            }
                        };
                        PageButtonsPanel.Children.Add(btn);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search failed: {ex.Message}");
            }
        }

        private async Task<PagedResponse<Trade>?> FetchPage(string cursor)
        {
            var url = $"http://localhost:7200/allocations/page?cursor={Uri.EscapeDataString(cursor)}";
            try
            {
                return await _httpClient.GetFromJsonAsync<PagedResponse<Trade>>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Page fetch failed: {ex.Message}");
                return null;
            }
        }

        private void OnLocalPageSliderChanged(object sender, ValueChangedEventArgs e)
        {
            int index = (int)Math.Round(e.NewValue);
            if (index != _localPageIndex)
            {
                UpdateLocalPage(index);
            }
        }

        private void OnLocalPagePrev(object sender, EventArgs e)
        {
            if (_localPageIndex > 0)
            {
                UpdateLocalPage(_localPageIndex - 1);
            }
        }

        private void OnLocalPageNext(object sender, EventArgs e)
        {
            int maxPage = (_currentPageRecords.Count + RecordsPerView - 1) / RecordsPerView;
            if (_localPageIndex < maxPage - 1)
            {
                UpdateLocalPage(_localPageIndex + 1);
            }
        }

        private void UpdateLocalPage(int localIndex)
        {
            _localPageIndex = localIndex;
            var slice = _currentPageRecords
                .Skip(_localPageIndex * RecordsPerView)
                .Take(RecordsPerView)
                .ToList();
            TradeCollection.ItemsSource = slice;

            int totalSubPages = (_currentPageRecords.Count + RecordsPerView - 1) / RecordsPerView;
            LocalPageSlider.Maximum = Math.Max(1, totalSubPages - 1);
            LocalPageSlider.Value = _localPageIndex;
            LocalPageLabel.Text = $"Sub-page: {_localPageIndex + 1} of {totalSubPages}";
        }
    }

    public enum TradeDirection { Hold = 0, Buy = 1, Sell = 2 }
    public enum TradeType { Limit = 0, Market, Stop, StopLimit, TrailingStop }
    public enum TradeDestination { BSE = 0, Cboe, CBOT, CME, CHX, ISE, MS4X, Nasdaq, NSX, NYSE, PHLX }

    public class Trade
    {
        public string Symbol { get; set; } = "";
        public long? Quantity { get; set; }
        public double? Price { get; set; }
        public DateTime Date { get; set; }
        public int Direction { get; set; }
        public int Destination { get; set; }
        public int Type { get; set; }
        public string BlockOrderCode { get; set; } = "";
    }

    public class TradeSearchPage
    {
        public int Page { get; set; }
        public int Offset { get; set; }
        public string Cursor { get; set; } = "";
    }

    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int Count { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<TradeSearchPage>? Pages { get; set; }
    }
}
