using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using megamarket.Models;
using megamarket.Services;
using Microsoft.Extensions.DependencyInjection;

namespace megamarket.ViewModels;

public partial class CustomerHomeViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _router;
    private readonly User _currentUser;

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();

    [ObservableProperty] private Category? _selectedCategory;
    [ObservableProperty] private bool _isLoading;
    
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private decimal? _minPrice;
    [ObservableProperty] private decimal? _maxPrice;
    
    
    [ObservableProperty] private bool _onlyInStock = false; 

    [ObservableProperty] private bool _hasMore = true;

    private int _requestVersion = 0;
    private int _skip = 0;
    private const int TakeCount = 10;

    public CustomerHomeViewModel(MainWindowViewModel router, User user)
    {
        _router = router;
        _currentUser = user;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            using var scope = App.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

            if (Categories.Count == 0)
            {
                var cats = await productService.GetCategoriesAsync();
                foreach (var c in cats) Categories.Add(c);
            }
            
            await LoadFirstPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedCategoryChanged(Category? value) => _ = LoadFirstPage();

    
    partial void OnOnlyInStockChanged(bool value) => _ = LoadFirstPage();

    private async Task LoadFirstPage()
    {
        _skip = 0;
        HasMore = true;
        _requestVersion++;
        int currentVersion = _requestVersion;

        IsLoading = true;
        Products.Clear();

        using var scope = App.Services.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

        var newProducts = await productService.GetProductsAsync(
            SelectedCategory?.Id, SearchText, MinPrice, MaxPrice, OnlyInStock, _skip, TakeCount);

        if (currentVersion != _requestVersion) return;

        if (newProducts.Count > 0)
        {
            foreach (var p in newProducts) Products.Add(p);
            _skip += newProducts.Count;
        }

        HasMore = newProducts.Count > 0;
        IsLoading = false;
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync() => await LoadFirstPage();

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        MinPrice = null;
        MaxPrice = null;
        OnlyInStock = false; 
        SelectedCategory = null;
        await LoadFirstPage();
    }

    [RelayCommand]
    private async Task LoadMoreProductsAsync()
    {
        if (IsLoading || !HasMore) return;

        IsLoading = true;
        int currentVersion = _requestVersion;

        using var scope = App.Services.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

        var newProducts = await productService.GetProductsAsync(
            SelectedCategory?.Id, SearchText, MinPrice, MaxPrice, OnlyInStock, _skip, TakeCount);

        if (currentVersion != _requestVersion) return;

        if (newProducts.Count > 0)
        {
            foreach (var p in newProducts) Products.Add(p);
            _skip += newProducts.Count;
        }

        HasMore = newProducts.Count == TakeCount;
        IsLoading = false;
    }

    [RelayCommand]
    private void OpenProduct(Product product)
    {
        if (product == null) return;
        _router.NavigateTo(new ProductDetailsViewModel(_router, product, _currentUser));
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        if (product == null) return;
        var cartService = App.Services.GetRequiredService<CartService>();
        cartService.AddToCart(product);
    }

    [RelayCommand]
    private void OpenCart() => _router.NavigateTo(new CartViewModel(_router, _currentUser));

    [RelayCommand]
    private void OpenProfile() => _router.NavigateTo(new ProfileViewModel(_router, _currentUser));

    [RelayCommand]
    private void Logout() => _router.NavigateTo(new LoginViewModel(_router));
}