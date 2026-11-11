using CommunityToolkit.Mvvm.ComponentModel;

namespace megamarket.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage = null!;

    public MainWindowViewModel()
    {
        CurrentPage = new LoginViewModel(this);
    }

    public void NavigateTo(ViewModelBase page) => CurrentPage = page;
    
}