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

public partial class AdminViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _router;
    private readonly User _currentUser;

    public ObservableCollection<Product> PendingProducts { get; } = new(); 
    public ObservableCollection<User> UsersList { get; } = new();
    public ObservableCollection<Address> AddressesList { get; } = new();

    [ObservableProperty] private string _statusMessage = "Панель администратора.";

    [ObservableProperty] private bool _isEditingAddress = false;
    [ObservableProperty] private int _editingAddressId;
    [ObservableProperty] private string _editCity = string.Empty;
    [ObservableProperty] private string _editStreet = string.Empty;
    [ObservableProperty] private string _editHouse = string.Empty;
    [ObservableProperty] private string _editApartment = string.Empty;

    [ObservableProperty] private string _newCity = string.Empty;
    [ObservableProperty] private string _newStreet = string.Empty;
    [ObservableProperty] private string _newHouse = string.Empty;

    public AdminViewModel(MainWindowViewModel router, User user)
    {
        _router = router;
        _currentUser = user;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadProductsAsync();
        await LoadUsersAsync();
        await LoadAddressesAsync();
    }

    
    private async Task LoadProductsAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var products = await context.Products
            .Include(p => p.Seller)
            .Include(p => p.Productcharacteristics)
            .Where(p => p.Statusid == 2) 
            .ToListAsync();

        PendingProducts.Clear();
        foreach (var p in products) PendingProducts.Add(p);
    }

    [RelayCommand]
    private async Task ApproveProduct(Product product)
    {
        if (product == null) return;
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var dbProduct = await context.Products.FindAsync(product.Id);
        if (dbProduct != null)
        {
            dbProduct.Statusid = 3; 
            await context.SaveChangesAsync();
            PendingProducts.Remove(product);
            StatusMessage = $"Товар '{product.Name}' одобрен!";
        }
    }

    [RelayCommand]
    private async Task RejectProduct(Product product)
    {
        if (product == null) return;
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var dbProduct = await context.Products.FindAsync(product.Id);
        if (dbProduct != null)
        {
            dbProduct.Statusid = 1; 
            await context.SaveChangesAsync();
            PendingProducts.Remove(product);
            StatusMessage = $"Товар '{product.Name}' отклонен.";
        }
    }

    
    private async Task LoadUsersAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();
        var users = await context.Users.ToListAsync();
        UsersList.Clear();
        foreach (var u in users) UsersList.Add(u);
    }

    [RelayCommand]
    private async Task ToggleBan(User user)
    {
        if (user == null) return;
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var dbUser = await context.Users.FindAsync(user.Id);
        if (dbUser != null)
        {
            dbUser.Isbanned = !dbUser.Isbanned;
            await context.SaveChangesAsync();
            var index = UsersList.IndexOf(user);
            if (index >= 0) UsersList[index] = dbUser;
            StatusMessage = dbUser.Isbanned == true ? $"⛔ {user.Email} забанен." : $"✅ {user.Email} разбанен.";
        }
    }

    
    private async Task LoadAddressesAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();
        var addresses = await context.Addresses.ToListAsync();
        AddressesList.Clear();
        foreach (var a in addresses) AddressesList.Add(a);
    }

    [RelayCommand]
    private void EditAddress(Address address)
    {
        if (address == null) return;
        EditingAddressId = address.Id;
        EditCity = address.City;
        EditStreet = address.Street;
        EditHouse = address.House;
        EditApartment = address.Apartment ?? string.Empty;
        IsEditingAddress = true;
    }

    [RelayCommand]
    private async Task SaveAddressAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var address = await context.Addresses.FindAsync(EditingAddressId);
        if (address != null)
        {
            address.City = EditCity;
            address.Street = EditStreet;
            address.House = EditHouse;
            address.Apartment = EditApartment;
            await context.SaveChangesAsync();
            StatusMessage = "Адрес обновлен!";
            await LoadAddressesAsync();
        }
        IsEditingAddress = false;
    }

    [RelayCommand]
    private void CancelEditAddress() => IsEditingAddress = false;

    [RelayCommand]
    private async Task DeleteAddressAsync(Address address)
    {
        if (address == null) return;
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var isUsedInOrders = await context.Orders.AnyAsync(o => o.Deliveryaddressid == address.Id);
        if (isUsedInOrders)
        {
            StatusMessage = "Нельзя удалить адрес: он используется в заказах.";
            return;
        }

        var dbAddress = await context.Addresses.FindAsync(address.Id);
        if (dbAddress != null)
        {
            context.Addresses.Remove(dbAddress);
            await context.SaveChangesAsync();
            StatusMessage = "🗑 Адрес удален.";
            await LoadAddressesAsync();
        }
    }

    [RelayCommand]
    private async Task AddAddressAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCity) || string.IsNullOrWhiteSpace(NewStreet))
        {
            StatusMessage = "Заполните город и улицу!";
            return;
        }

        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MegamarketContext>();

        var address = new Address
        {
            Region = NewCity,
            City = NewCity,
            Street = NewStreet,
            House = NewHouse,
            Postcode = "000000"
        };
        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        StatusMessage = "Адрес добавлен!";
        NewCity = ""; NewStreet = ""; NewHouse = "";
        await LoadAddressesAsync();
    }

    [RelayCommand]
    private void GoBack() => _router.NavigateTo(new LoginViewModel(_router));
}