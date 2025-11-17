using FleetManager.Services;
using FleetManager.Data;           // pas indispensable ici pour l’instant
using Microsoft.EntityFrameworkCore;

namespace FleetManager.ViewModels;

public class AcceuilViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    public AcceuilViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;
    }

    public string WelcomeText
    {
        get
        {
            if (_state.CurrentUser is null)
                return "Bienvenue";

            return $"Bienvenue {_state.CurrentUser.Nom}";
        }
    }
}