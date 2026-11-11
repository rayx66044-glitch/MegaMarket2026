using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using megamarket.Models;
using megamarket.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace megamarket.ViewModels;

public partial class ProfileViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _router;
    private readonly User _currentUser;
    private readonly OrderService _orderService;

    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _userEmail = string.Empty;
    [ObservableProperty] private decimal _bonusBalance;
    [ObservableProperty] private string _statusMessage = string.Empty;

    
    [ObservableProperty] private bool _isReviewing = false;
    [ObservableProperty] private int _reviewProductId;
    [ObservableProperty] private string _reviewProductName = string.Empty;
    [ObservableProperty] private int _reviewRating = 5; 
    [ObservableProperty] private string _reviewText = string.Empty;

    private readonly HashSet<int> _reviewedProductIds = new();
    private readonly HashSet<int> _hiddenOrderIds = new();

    public ObservableCollection<Order> Orders { get; } = new();

    public ProfileViewModel(MainWindowViewModel router, User user)
    {
        _router = router;
        _currentUser = user;
        _orderService = App.Services.GetRequiredService<OrderService>();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var profile = await Task.Run(() => context.Customerprofiles.FirstOrDefault(c => c.Userid == _currentUser.Id));
        if (profile != null)
        {
            UserName = $"{profile.Lastname} {profile.Firstname}";
            BonusBalance = profile.Bonuspointsbalance;

            var reviewedIds = await context.Reviews
                .Where(r => r.Customerid == profile.Id)
                .Select(r => r.Productid)
                .ToListAsync();

            _reviewedProductIds.Clear();
            foreach (var id in reviewedIds) _reviewedProductIds.Add(id);
        }
        UserEmail = _currentUser.Email;

        var orders = await _orderService.GetUserOrdersAsync(_currentUser.Id);
        Orders.Clear();
        foreach (var order in orders) 
        {
            if (!_hiddenOrderIds.Contains(order.Id)) Orders.Add(order);
        }
    }

    
    [RelayCommand]
    private async Task CancelOrder(Order order)
    {
        if (order == null) return;

        bool success = await _orderService.CancelOrderAsync(order.Id);
        
        if (success)
        {
            StatusMessage = $"Заказ №{order.Id} отменен. Товары возвращены на склад.";
            var orders = await _orderService.GetUserOrdersAsync(_currentUser.Id);
            Orders.Clear();
            foreach (var o in orders) 
            {
                if (!_hiddenOrderIds.Contains(o.Id)) Orders.Add(o);
            }
        }
        else
        {
            StatusMessage = "Не удалось отменить заказ.";
        }
    }

    
    [RelayCommand]
    private void HideOrder(Order order)
    {
        if (order == null) return;
        _hiddenOrderIds.Add(order.Id);
        Orders.Remove(order);
        StatusMessage = $"Заказ №{order.Id} скрыт из истории.";
    }

    
    [RelayCommand]
    private void StartReview(Orderitem item)
    {
        if (item == null) return;

        var order = Orders.FirstOrDefault(o => o.Orderitems.Contains(item));

        
        if (order == null || (order.Statusid != 5 && order.Statusid != 7))
        {
            StatusMessage = "Оставить отзыв можно только после получения заказа.";
            return;
        }

        if (_reviewedProductIds.Contains(item.Productid))
        {
            StatusMessage = "Вы уже оставили отзыв на этот товар.";
            return;
        }

        ReviewProductId = item.Productid;
        ReviewProductName = item.Product.Name;
        ReviewRating = 5; 
        ReviewText = string.Empty;
        IsReviewing = true;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void CancelReview()
    {
        IsReviewing = false;
        ReviewText = string.Empty;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SubmitReview()
    {
        if (string.IsNullOrWhiteSpace(ReviewText))
        {
            StatusMessage = "Напишите текст отзыва!";
            return;
        }

        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var customerProfile = context.Customerprofiles.FirstOrDefault(c => c.Userid == _currentUser.Id);
        if (customerProfile == null) return;

        
        var review = new Review
        {
            Productid = ReviewProductId,
            Customerid = customerProfile.Id,
            Rating = (short)ReviewRating, 
            Textreview = ReviewText,
            Createdat = DateTime.UtcNow
        };

        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        _reviewedProductIds.Add(ReviewProductId);
        StatusMessage = "Спасибо за ваш отзыв!";
        IsReviewing = false;
    }

    [RelayCommand]
    private void GoBack() => _router.NavigateTo(new CustomerHomeViewModel(_router, _currentUser));
    
    [RelayCommand]
    private void Logout() => _router.NavigateTo(new LoginViewModel(_router));
}
// Улучшенный личный кабинет