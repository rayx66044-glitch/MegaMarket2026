using System;
using System.Collections.Generic;

namespace megamarket.Models;

public partial class Orderstatus
{
    public int Id { get; set; }

    public int Orderid { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Changedat { get; set; }

    public virtual Order Order { get; set; } = null!;
}
