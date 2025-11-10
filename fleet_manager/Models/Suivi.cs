using System;
using System.Collections.Generic;
using FleetManager.Models;

namespace FleetManager.Models;

public class Suivi
{
    public int Id { get; set; }
    public DateTime DateSuivi { get; set; }
    public int km_depart { get; set; }
    public int km_arrivee { get; set; }
    public string destiantion { get; set; }
    public string commentaire { get; set; }
    public List<User> id_users { get; set; }
    public List<Vehicule> vehicules { get; set; }
    
}