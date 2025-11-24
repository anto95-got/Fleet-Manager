using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FleetManager.Models;


namespace FleetManager.Views;   // ✅ doit être "Views", pas "ViewModels"

public partial class SuivieView : UserControl
{
    public SuivieView()
    {
        InitializeComponent();   // ✅ la vraie méthode générée par Avalonia
    }
}