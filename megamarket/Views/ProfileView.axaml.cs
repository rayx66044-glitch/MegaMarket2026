using Avalonia.Controls;
using Avalonia.Interactivity;
using megamarket.Models;
using megamarket.ViewModels;

namespace megamarket.Views;

public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
    }

    // 👇 Обработчик нажатия на кнопку отзыва
    private void OnReviewClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Orderitem item)
        {
            if (DataContext is ProfileViewModel viewModel)
            {
                // Вызываем команду напрямую из ViewModel
                viewModel.StartReviewCommand.Execute(item);
            }
        }
    }
}