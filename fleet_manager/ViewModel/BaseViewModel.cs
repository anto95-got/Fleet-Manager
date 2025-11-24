using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FleetManager.Services;

namespace FleetManager.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    // Permet au ViewModel d'accéder au NavigationService global
    protected NavigationService Nav => App.GlobalNav;

    // 🔥 Commandes globales disponibles dans TOUTES les pages
    public ICommand Logout => Nav.LogoutCommand;
    public ICommand GoHome => Nav.GoHomeCommand;
    public ICommand GoVehicules => Nav.GoVehiculesCommand;
    public ICommand GoSuivie => Nav.GoSuivieCommand;

    // ============================
    //      NOTIFICATIONS MVVM
    // ============================
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}