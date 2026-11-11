using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using megamarket.Models;
using megamarket.Services;
using Microsoft.Extensions.DependencyInjection;

namespace megamarket.ViewModels;

public partial class CheckoutViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _router;
    private readonly User _currentUser;
    private readonly CartService _cartService;
    private readonly OrderService _orderService;
    private readonly bool _isPromoApplied; 

    [ObservableProperty] private string _newCity = string.Empty;
    [ObservableProperty] private string _newStreet = string.Empty;
    [ObservableProperty] private string _newHouse = string.Empty;
    [ObservableProperty] private string _newApartment = string.Empty;
    
    [ObservableProperty] private bool _isAddressValid;
    
    [ObservableProperty] private decimal _bonusBalance;
    [ObservableProperty] private decimal _usedBonuses;
    
    [ObservableProperty] private decimal _subTotal;
    [ObservableProperty] private decimal _deliveryTotal;
    [ObservableProperty] private decimal _finalTotal;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isProcessing = false;

    public CheckoutViewModel(MainWindowViewModel router, User user, bool isPromoApplied = false)
    {
        _router = router;
        _currentUser = user;
        _isPromoApplied = isPromoApplied;
        _cartService = App.Services.GetRequiredService<CartService>();
        _orderService = App.Services.GetRequiredService<OrderService>();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        
        var userProfile = await Task.Run(() => context.Customerprofiles.FirstOrDefault(c => c.Userid == _currentUser.Id));
        if (userProfile != null)
        {
            BonusBalance = userProfile.Bonuspointsbalance;
        }

        
        SubTotal = _cartService.Items.Sum(i => i.TotalPrice);
        
        
        if (_isPromoApplied)
        {
            SubTotal = SubTotal * 0.9m; 
        }

        
        int totalItemsCount = _cartService.Items.Sum(i => i.Quantity);
        if (totalItemsCount == 0)
            DeliveryTotal = 0;
        else if (SubTotal > 5000)
            DeliveryTotal = 100;
        else
            DeliveryTotal = Math.Min(500 + (totalItemsCount - 1) * 50, 1000);
        
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        FinalTotal = SubTotal + DeliveryTotal - UsedBonuses;
        if (FinalTotal < 0) FinalTotal = 0;
    }

    partial void OnUsedBonusesChanged(decimal value)
    {
        if (value > BonusBalance) UsedBonuses = BonusBalance;
        else if (value < 0) UsedBonuses = 0;
        RecalculateTotal();
    }

    partial void OnNewCityChanged(string value) => ValidateAddress();
    partial void OnNewStreetChanged(string value) => ValidateAddress();
    partial void OnNewHouseChanged(string value) => ValidateAddress();

    private void ValidateAddress()
    {
        IsAddressValid = !string.IsNullOrWhiteSpace(NewCity) && 
                         !string.IsNullOrWhiteSpace(NewStreet) && 
                         !string.IsNullOrWhiteSpace(NewHouse);
    }

    [RelayCommand]
    private async Task SubmitOrder()
    {
        if (!IsAddressValid)
        {
            StatusMessage = "Заполните все поля адреса (Город, Улица, Дом)!";
            return;
        }

        IsProcessing = true;
        try
        {
            var newAddr = await _orderService.CreateAddressAsync(NewCity, NewStreet, NewHouse, NewApartment);

            
            int orderId = await _orderService.CreateOrderAsync(
                _currentUser.Id, 
                newAddr.Id, 
                _cartService.Items.ToList(), 
                FinalTotal, 
                DeliveryTotal, 
                UsedBonuses);

            _cartService.Clear();
            
            StatusMessage = $"Заказ №{orderId} успешно оформлен!";
            await Task.Delay(2000);
            _router.NavigateTo(new CustomerHomeViewModel(_router, _currentUser));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
        IsProcessing = false;
    }

    [RelayCommand]
    private void GoBack() => _router.NavigateTo(new CartViewModel(_router, _currentUser));
}