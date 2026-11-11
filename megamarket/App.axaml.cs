using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using megamarket.Models;
using megamarket.Services;
using megamarket.ViewModels;
using megamarket.Views;
using System;

namespace megamarket;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            
            // Регистрация БД
            services.AddDbContext<MegamarketContext>();

            // 👇 РЕГИСТРАЦИЯ ВСЕХ СЕРВИСОВ
            services.AddScoped<AuthService>();
            services.AddScoped<ProductService>();
            services.AddScoped<OrderService>(); // <-- ВОТ ЭТА СТРОКА!
            services.AddScoped<WarehouseService>();
            services.AddScoped<AnalyticsService>(); 
            
            // Корзина должна быть Singleton, чтобы данные не терялись при переходах
            services.AddSingleton<CartService>();

            Services = services.BuildServiceProvider();

            var mainVm = new MainWindowViewModel();
            desktop.MainWindow = new MainWindow { DataContext = mainVm };
        }

        base.OnFrameworkInitializationCompleted();
    }
}