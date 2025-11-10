using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Models;

public class Concerner
{
    public int Id { get; set; }
    public DateTime date_plein { get; set; }
    [Precision(6, 2)]
    public decimal litres { get; set; }
    public List<Vehicule> id_vehicules { get; set; }
    public List<Suivi> id_suivi { get; set; }
}