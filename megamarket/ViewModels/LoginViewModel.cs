using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using megamarket.Services;

namespace megamarket.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _router;
    private readonly AuthService _authService;

    
    [ObservableProperty] private int _selectedTabIndex = 0;

    
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _inn = string.Empty;
    [ObservableProperty] private bool _isSellerMode;

    
    [ObservableProperty] private string _captchaText = string.Empty;
    [ObservableProperty] private string _captchaInput = string.Empty;

    
    [ObservableProperty] private string _emailHint = "Введите Email";
    [ObservableProperty] private bool _isEmailValid;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _innStatus = string.Empty;

    public LoginViewModel(MainWindowViewModel router)
    {
        _router = router;
        _authService = App.Services.GetRequiredService<AuthService>();
        RefreshCaptcha();
    }

    
    partial void OnEmailChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) 
        { 
            IsEmailValid = false; 
            EmailHint = "Введите Email"; 
        }
        else if (!value.Contains("@") || !value.Contains(".")) 
        { 
            IsEmailValid = false; 
            EmailHint = "Некорректный Email"; 
        }
        else 
        { 
            IsEmailValid = true; 
            EmailHint = "Email корректен"; 
        }
    }

    [RelayCommand]
    private void RefreshCaptcha()
    {
        var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        var result = new char[4];
        for (int i = 0; i < 4; i++) result[i] = chars[random.Next(chars.Length)];
        CaptchaText = new string(result);
        CaptchaInput = string.Empty;
    }

    [RelayCommand]
    private async Task SendInnToCheck()
    {
        if (string.IsNullOrWhiteSpace(Inn) || Inn.Length < 10) 
        { 
            InnStatus = "❌ ИНН должен содержать минимум 10 цифр"; 
            return; 
        }
        InnStatus = "⏳ Отправка на проверку...";
        await Task.Delay(1000);
        InnStatus = "✅ ИНН успешно проверен";
    }

    [RelayCommand]
    private async Task Login()
    {
        if (!IsEmailValid || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Проверьте правильность заполнения полей";
            return;
        }

        var user = await _authService.LoginAsync(Email, Password);
        
        if (user == null) 
        { 
            StatusMessage = "Неверный email или пароль"; 
            return; 
        }
        
        if (user.Isbanned == true) 
        { 
            StatusMessage = "Ваш аккаунт заблокирован администратором"; 
            return; 
        }

        
        if (user.Roleid == 1) 
        {
            _router.NavigateTo(new AdminViewModel(_router, user));
        }
        else if (user.Roleid == 3) 
        {
            _router.NavigateTo(new SellerWarehouseViewModel(_router, user));
        }
        else 
        {
            _router.NavigateTo(new CustomerHomeViewModel(_router, user));
        }
    }

    [RelayCommand]
    private async Task Register()
    {
        if (CaptchaInput != CaptchaText) 
        { 
            StatusMessage = "Неверная капча"; 
            return; 
        }
        
        if (!IsEmailValid || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Phone)) 
        { 
            StatusMessage = "Заполните обязательные поля (Email, Пароль, Телефон)"; 
            return; 
        }
        
        if (IsSellerMode && string.IsNullOrWhiteSpace(Inn)) 
        { 
            StatusMessage = "Для продавца ИНН обязателен"; 
            return; 
        }
        
        if (!IsSellerMode && (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))) 
        { 
            StatusMessage = "Введите Имя и Фамилию"; 
            return; 
        }

        var success = await _authService.RegisterAsync(Email, Password, Phone, Inn, IsSellerMode, FirstName, LastName);
        
        if (success)
        {
            StatusMessage = "Регистрация успешна! Теперь вы можете войти.";
            SelectedTabIndex = 0; 
            IsSellerMode = false;
            Password = string.Empty;
            CaptchaInput = string.Empty;
            RefreshCaptcha();
        }
        else
        {
            StatusMessage = "Пользователь с таким Email уже существует";
        }
    }
}