namespace FleetManager.Models;
using System.Windows.Input;


    public class MenuItem
    {
        public string Title { get; set; }
        public ICommand Command { get; set; }
    }
