using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using megamarket.Models;
using megamarket.Services;
using Microsoft.Extensions.DependencyInjection;

namespace megamarket.ViewModels;

public partial class ProductDetailsViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _router;
    private readonly User _currentUser;

    [ObservableProperty] private Product? _currentProduct;
    [ObservableProperty] private int _selectedTabIndex = 0;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isLoading = true;

    public ProductDetailsViewModel(MainWindowViewModel router, Product product, User user)
    {
        _router = router;
        _currentUser = user;
        _ = LoadProductDetailsAsync(product.Id);
    }

    private async Task LoadProductDetailsAsync(int productId)
    {
        IsLoading = true;
        try
        {
            using var scope = App.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

            var fullProduct = await productService.GetProductDetailsAsync(productId);
            
            if (fullProduct != null)
            {
                CurrentProduct = fullProduct;
            }
            else
            {
                StatusMessage = "Товар не найден";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            StatusMessage = "Ошибка загрузки данных";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _router.NavigateTo(new CustomerHomeViewModel(_router, _currentUser)); 
    }

    [RelayCommand]
    private void AddToCart()
    {
        if (CurrentProduct != null)
        {
            var cartService = App.Services.GetRequiredService<CartService>();
            cartService.AddToCart(CurrentProduct);
            StatusMessage = "Товар добавлен в корзину!";
        }
    }

    [RelayCommand]
    private void GoToCart()
    {
        _router.NavigateTo(new CartViewModel(_router, _currentUser));
    }
}