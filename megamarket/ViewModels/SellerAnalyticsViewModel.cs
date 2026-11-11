using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using megamarket.Models;
using megamarket.Services;
using Microsoft.Extensions.DependencyInjection;

namespace megamarket.ViewModels;

public partial class SellerAnalyticsViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _router;
    private readonly User _currentUser;
    private readonly AnalyticsService _analyticsService;

    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private int _totalOrders;
    [ObservableProperty] private string _sellerName = string.Empty;

    
    public ISeries[] RevenueSeries { get; set; } = Array.Empty<ISeries>();
    public Axis[] XAxes { get; set; } = Array.Empty<Axis>();
    
    
    public ISeries[] TopProductsSeries { get; set; } = Array.Empty<ISeries>();
    public Axis[] TopProductsYAxes { get; set; } = Array.Empty<Axis>();

    public SellerAnalyticsViewModel(MainWindowViewModel router, User user)
    {
        _router = router;
        _currentUser = user;
        _analyticsService = App.Services.GetRequiredService<AnalyticsService>();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();
        var seller = context.Sellerprofiles.FirstOrDefault(s => s.Userid == _currentUser.Id);

        if (seller == null) 
        {
            SellerName = "Ошибка: продавец не найден";
            return;
        }
        
        SellerName = seller.Companyname;
        int sellerId = seller.Id;

        
        var stats = await _analyticsService.GetTotalStatsAsync(sellerId);
        TotalRevenue = stats.Revenue;
        TotalOrders = stats.OrdersCount;

        
        var salesByDay = await _analyticsService.GetSalesByDayAsync(sellerId);
        
        RevenueSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = salesByDay.Values.ToArray(),
                Fill = new SolidColorPaint(SKColors.LightBlue.WithAlpha(50)),
                Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 4 },
                GeometrySize = 10,
                GeometryStroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 2 },
                Name = "Выручка"
            }
        };

        XAxes = new[]
        {
            new Axis 
            { 
                Labels = salesByDay.Keys.ToArray(), 
                TextSize = 14,
                LabelsPaint = new SolidColorPaint(SKColors.Gray)
            }
        };

        
        var topProducts = await _analyticsService.GetTopProductsAsync(sellerId);
        
        TopProductsSeries = new ISeries[]
        {
            new RowSeries<int> 
            {
                Values = topProducts.Values.ToArray(),
                Fill = new SolidColorPaint(SKColors.MediumSeaGreen),
                MaxBarWidth = 40,
                Name = "Продано (шт.)"
            }
        };

        TopProductsYAxes = new[]
        {
            new Axis 
            { 
                Labels = topProducts.Keys.ToArray(), 
                TextSize = 14,
                LabelsPaint = new SolidColorPaint(SKColors.Black)
            }
        };

        
        OnPropertyChanged(nameof(RevenueSeries));
        OnPropertyChanged(nameof(XAxes));
        OnPropertyChanged(nameof(TopProductsSeries));
        OnPropertyChanged(nameof(TopProductsYAxes));
    }

    [RelayCommand]
    private void GoToWarehouse() => _router.NavigateTo(new SellerWarehouseViewModel(_router, _currentUser));

    [RelayCommand]
    private void Logout() => _router.NavigateTo(new LoginViewModel(_router));
}