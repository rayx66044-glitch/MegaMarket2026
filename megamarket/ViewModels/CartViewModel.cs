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

public partial class SellerCartGroup : ObservableObject
{
    public string SellerName { get; set; } = string.Empty;
    public ObservableCollection<CartItem> Items { get; set; } = new();
    
    [ObservableProperty] private decimal _deliveryCost;
    [ObservableProperty] private decimal _totalPrice;

    public void Recalculate()
    {
        TotalPrice = Items.Sum(i => i.TotalPrice);
        
        int totalItemsCount = Items.Sum(i => i.Quantity);
        if (totalItemsCount == 0)
        {
            DeliveryCost = 0;
        }
        else if (TotalPrice > 5000)
        {
            DeliveryCost = 100; 
        }
        else
        {
            
            DeliveryCost = Math.Min(500 + (totalItemsCount - 1) * 50, 1000);
        }
    }
}

public partial class CartViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _router;
    private readonly User _currentUser;
    private readonly CartService _cartService;

    public ObservableCollection<SellerCartGroup> SellerGroups { get; } = new();

    [ObservableProperty] private string _promoCode = string.Empty;
    [ObservableProperty] private decimal _finalTotal;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _canCheckout = true;

    public bool IsPromoApplied { get; private set; } = false; 
    private decimal _baseTotal = 0;

    public CartViewModel(MainWindowViewModel router, User user)
    {
        _router = router;
        _currentUser = user;
        _cartService = App.Services.GetRequiredService<CartService>();
        _ = LoadCartAsync();
    }

    private async Task LoadCartAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        
        foreach (var item in _cartService.Items)
        {
            var dbStock = context.Productstocks.FirstOrDefault(ps => ps.Productid == item.Product.Id);
            if (dbStock != null)
            {
                item.MaxStock = dbStock.Quantityinstock;
                if (item.Quantity > item.MaxStock) item.Quantity = item.MaxStock;
            }
        }

        
        var outOfStockItems = _cartService.Items.Where(i => i.MaxStock == 0).ToList();
        foreach (var item in outOfStockItems)
        {
            _statusMessage = $"⚠️ Товар '{item.Product.Name}' закончился и был удален из корзины.";
            _cartService.Items.Remove(item);
        }

        SellerGroups.Clear();

        var groups = _cartService.Items
            .GroupBy(i => i.Product.Seller.Companyname) 
            .Select(g => new SellerCartGroup
            {
                SellerName = g.Key,
                Items = new ObservableCollection<CartItem>(g)
            });

        foreach (var group in groups)
        {
            group.Recalculate();
            SellerGroups.Add(group);
        }

        var overstocked = _cartService.Items.FirstOrDefault(i => i.Quantity > i.MaxStock);
        if (overstocked != null)
        {
            _statusMessage = $"⚠️ Товара '{overstocked.Product.Name}' на складе только {overstocked.MaxStock} шт.";
            CanCheckout = false;
        }
        else
        {
            CanCheckout = _cartService.Items.Any();
        }

        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        
        decimal itemsTotal = SellerGroups.Sum(g => g.TotalPrice);
        
        
        decimal deliveryTotal = SellerGroups.Sum(g => g.DeliveryCost);

        
        if (IsPromoApplied)
        {
            itemsTotal = itemsTotal * 0.9m;
        }

        
        FinalTotal = itemsTotal + deliveryTotal;
    }

    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        if (item == null) return;
        if (item.Quantity >= item.MaxStock)
        {
            StatusMessage = $"На складе всего {item.MaxStock} шт.";
            return;
        }

        item.Quantity += 1;
        var group = SellerGroups.FirstOrDefault(g => g.Items.Contains(item));
        group?.Recalculate();
        RecalculateTotal();
    }

    [RelayCommand]
    private void DecreaseQuantity(CartItem item)
    {
        if (item == null) return;

        if (item.Quantity > 1)
        {
            item.Quantity -= 1;
            var group = SellerGroups.FirstOrDefault(g => g.Items.Contains(item));
            group?.Recalculate();
            RecalculateTotal();
        }
        else
        {
            RemoveItem(item);
        }
    }

    [RelayCommand]
    private void RemoveItem(CartItem item)
    {
        if (item == null) return;
        _cartService.RemoveFromCart(item.Product.Id);
        _ = LoadCartAsync(); 
    }

    [RelayCommand]
    private void ApplyPromoCode()
    {
        if (IsPromoApplied)
        {
            StatusMessage = "Промокод уже применен!";
            return;
        }

        if (PromoCode.ToUpper() == "SALE10")
        {
            IsPromoApplied = true;
            RecalculateTotal();
            StatusMessage = "Промокод SALE10 применен (скидка 10%)";
        }
        else
        {
            StatusMessage = " еверный промокод";
        }
    }

    [RelayCommand]
    private void GoToCheckout()
    {
        
        _router.NavigateTo(new CheckoutViewModel(_router, _currentUser, IsPromoApplied));
    }

    [RelayCommand]
    private void GoBack() => _router.NavigateTo(new CustomerHomeViewModel(_router, _currentUser));
}