using System;
using System.Collections.Generic;

namespace megamarket.Models;

public partial class Orderstatusesdict
{
    public int Id { get; set; }

    public string Statusname { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
