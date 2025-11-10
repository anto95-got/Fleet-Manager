using FleetManager.Services;

namespace FleetManager.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private object? _currentView;
    public object? CurrentView
    {
        get => _currentView;
        set { _currentView = value; OnPropertyChanged(); }
    }

    public AppState State { get; }

    public MainWindowViewModel(AppState state) => State = state;
}