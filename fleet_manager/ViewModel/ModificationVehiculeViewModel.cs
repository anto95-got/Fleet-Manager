using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Services;
using FleetManager.Data;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.ViewModels;

public class ModificationVehiculeViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    private string _imatricule = "";
    private string _marque = "";
    private string _modele = "";
    private int _annee ;
    private int _kilometrage ; 
    private string _statut = "";
    private string _error = "";

    public string imatricule { get => _imatricule; set { _imatricule = value; OnPropertyChanged(); } }
    public string marque { get => _marque; set { _marque = value; OnPropertyChanged(); } }
    public string modele { get => _modele; set { _modele = value; OnPropertyChanged(); } }
    public int annee { get => _annee; set { _annee = value; OnPropertyChanged(); } }
    public int kilometrage { get => _kilometrage; set { _kilometrage = value; OnPropertyChanged(); } }
    public string statut { get => _statut; set { _statut = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

    
    public ICommand AjouterCommand { get; }
    public ICommand GoToHome { get; }
    
    
    
    
    public ModificationVehiculeViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;
        
       // AjouterCommand = new Command(AjouterVehicule);
       GoToHome = new RelayCommand(() => _nav.GoToHome());
       // AjouterCommand = new RelayCommand(async () => await AjouterVehicule());

    }
    
    
}