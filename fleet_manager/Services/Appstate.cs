using FleetManager.Models;

namespace FleetManager.Services;

public class AppState
{
    public User? CurrentUser { get; set; }
    public bool IsAuthenticated => CurrentUser != null;
    
    
}