using FleetManager.Data;
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
    public FleetDbContext Db { get; }
    public NavigationService Nav { get; }

    public MainWindowViewModel()
    {
        // Initialise l'état global
        State = new AppState();

        // Initialise le DbContext
        Db = new FleetDbContext();

        // Initialise le système de navigation
        Nav = new NavigationService(this);

        // Afficher la page Login au lancement
        CurrentView = new ConnexionViewModel(Nav, State);
    }
}