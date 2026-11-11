using System;
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

public partial class EditableCharacteristic : ObservableObject
{
    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private string _value = string.Empty;
}

public partial class SellerWarehouseViewModel : ViewModelBase
{
    public ObservableCollection<Category> Categories { get; } = new(); 
    private readonly MainWindowViewModel _router;
    private readonly User _currentUser;
    private readonly OrderService _orderService;
    private int _sellerId;

    [ObservableProperty] private ObservableCollection<Productstock> _stocks = new();
    [ObservableProperty] private ObservableCollection<Order> _orders = new();
    
    public ObservableCollection<Warehouse> Warehouses { get; } = new();
    public ObservableCollection<Orderstatusesdict> OrderStatusesDicts { get; } = new();
    public ObservableCollection<EditableCharacteristic> EditCharacteristics { get; } = new();

    [ObservableProperty] private Warehouse? _selectedWarehouse;
    [ObservableProperty] private string _statusMessage = "Загрузка...";

    [ObservableProperty] private string _newName = string.Empty;
    [ObservableProperty] private string _newSku = string.Empty;
    [ObservableProperty] private Category? _selectedCategory; 
    [ObservableProperty] private decimal _newPrice;
    [ObservableProperty] private string _newDescription = string.Empty;

    [ObservableProperty] private bool _isEditing = false;
    [ObservableProperty] private int _editingProductId;
    [ObservableProperty] private string _editName = string.Empty;
    [ObservableProperty] private string _editDescription = string.Empty;
    [ObservableProperty] private decimal _editPrice; 

    public SellerWarehouseViewModel(MainWindowViewModel router, User user)
    {
        _router = router;
        _currentUser = user;
        _orderService = App.Services.GetRequiredService<OrderService>();

        OrderStatusesDicts.Add(new Orderstatusesdict { Id = 3, Statusname = "Сборка" });
        OrderStatusesDicts.Add(new Orderstatusesdict { Id = 4, Statusname = "В пути" });
        OrderStatusesDicts.Add(new Orderstatusesdict { Id = 5, Statusname = "Доставлено" });
        OrderStatusesDicts.Add(new Orderstatusesdict { Id = 6, Statusname = "Отменен" });
        OrderStatusesDicts.Add(new Orderstatusesdict { Id = 7, Statusname = "Получен" });

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try 
        {
            using var scope = App.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();
            
            var seller = context.Sellerprofiles.FirstOrDefault(s => s.Userid == _currentUser.Id);
            if (seller == null) { StatusMessage = "ОШИБКА: Продавец не найден!"; return; }
            _sellerId = seller.Id;
            
            var cats = context.Categories.Where(c => c.Isactive == true && c.Parentcategoryid == null).ToList();
            Categories.Clear();
            foreach (var c in cats) Categories.Add(c);
            SelectedCategory = Categories.FirstOrDefault();

            var whs = context.Warehouses.ToList();
            Warehouses.Clear();
            foreach (var w in whs) Warehouses.Add(w);
            SelectedWarehouse = Warehouses.FirstOrDefault();

            var orders = await _orderService.GetSellerOrdersAsync(_sellerId);
            Orders = new ObservableCollection<Order>(orders);
        }
        catch (Exception ex)
        {
            StatusMessage = $"ОШИБКА: {ex.Message}";
        }
    }

    partial void OnSelectedWarehouseChanged(Warehouse? value) => LoadStocks();

    private void LoadStocks()
    {
        if (SelectedWarehouse == null) return;
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var stocks = context.Productstocks
            .Include(ps => ps.Product)
            .Where(ps => ps.Warehouseid == SelectedWarehouse.Id && ps.Product.Sellerid == _sellerId)
            .ToList();

        Stocks = new ObservableCollection<Productstock>(stocks);
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewSku) || NewPrice <= 0)
        {
            StatusMessage = "Заполните обязательные поля!";
            return;
        }

        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var newProduct = new Product
        {
            Name = NewName, 
            Sku = NewSku, 
            Baseprice = NewPrice, 
            Sellerid = _sellerId, 
            Categoryid = SelectedCategory?.Id ?? 1, 
            Statusid = 2, 
            Currentdiscount = 0,
            Description = NewDescription
        };
        context.Products.Add(newProduct);
        await context.SaveChangesAsync();

        var stock = new Productstock 
        { 
            Productid = newProduct.Id,
            Warehouseid = SelectedWarehouse!.Id, 
            Quantityinstock = 0
        };
        context.Productstocks.Add(stock);
        await context.SaveChangesAsync();

        LoadStocks(); 
        StatusMessage = "Товар отправлен на модерацию!";
        NewName = ""; NewSku = ""; NewPrice = 0; NewDescription = "";
    }

    [RelayCommand]
    private async Task EditProduct(Productstock stock)
    {
        if (stock == null) return;
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var product = await context.Products
            .Include(p => p.Productcharacteristics)
            .FirstOrDefaultAsync(p => p.Id == stock.Productid);

        if (product == null) return;

        EditingProductId = product.Id;
        EditName = product.Name;
        EditDescription = product.Description ?? string.Empty;
        EditPrice = product.Baseprice; 

        EditCharacteristics.Clear();
        foreach (var c in product.Productcharacteristics)
        {
            EditCharacteristics.Add(new EditableCharacteristic { Key = c.Keyname, Value = c.Value });
        }

        IsEditing = true; 
    }

    [RelayCommand]
    private async Task DeleteProductAsync(Productstock stock)
    {
        if (stock == null) return;
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var product = await context.Products.FindAsync(stock.Productid);
        if (product != null)
        {
            var stocks = context.Productstocks.Where(s => s.Productid == product.Id);
            context.Productstocks.RemoveRange(stocks);
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            
            LoadStocks(); 
            StatusMessage = "Товар удален!";
        }
    }

    [RelayCommand]
    private void AddCharacteristic() => EditCharacteristics.Add(new EditableCharacteristic { Key = "", Value = "" });

    [RelayCommand]
    private void RemoveCharacteristic(EditableCharacteristic charToRemove)
    {
        if (charToRemove != null) EditCharacteristics.Remove(charToRemove);
    }

    [RelayCommand]
    private async Task SaveProductAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var product = await context.Products
            .Include(p => p.Productcharacteristics)
            .FirstOrDefaultAsync(p => p.Id == EditingProductId);

        if (product == null) return;

        product.Name = EditName;
        product.Description = EditDescription;
        product.Baseprice = EditPrice; 
        product.Statusid = 2; 

        context.Productcharacteristics.RemoveRange(product.Productcharacteristics);
        
        foreach (var c in EditCharacteristics)
        {
            if (!string.IsNullOrWhiteSpace(c.Key))
            {
                context.Productcharacteristics.Add(new Productcharacteristic
                {
                    Productid = product.Id,
                    Keyname = c.Key,
                    Value = c.Value
                });
            }
        }

        await context.SaveChangesAsync();
        
        IsEditing = false;
        LoadStocks();
        StatusMessage = "Товар отправлен на модерацию!";
    }

    [RelayCommand]
    private void CancelEdit() { IsEditing = false; EditCharacteristics.Clear(); }

    [RelayCommand]
    private async Task SaveStocksAsync()
    {
        try
        {
            using var scope = App.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

            foreach (var stock in Stocks)
            {
                var dbStock = await context.Productstocks.FindAsync(stock.Id);
                if (dbStock != null) dbStock.Quantityinstock = stock.Quantityinstock;
            }

            await context.SaveChangesAsync();
            StatusMessage = "Остатки сохранены!";
        }
        catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task ChangeOrderStatus(Order order)
    {
        if (order == null) return;
        var statusDict = OrderStatusesDicts.FirstOrDefault(s => s.Id == order.Statusid);
        string statusName = statusDict?.Statusname ?? "Unknown";

        await _orderService.UpdateOrderStatusAsync(order.Id, order.Statusid, statusName);
        StatusMessage = $"Статус заказа №{order.Id} изменен на '{statusName}'";
    }

    [RelayCommand]
    private void GoToAnalytics() => _router.NavigateTo(new SellerAnalyticsViewModel(_router, _currentUser));
    [RelayCommand]
    private void Logout() => _router.NavigateTo(new LoginViewModel(_router));
    [RelayCommand]
    private void GoBack() => _router.NavigateTo(new LoginViewModel(_router));
}