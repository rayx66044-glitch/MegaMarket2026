using System;
using System.Collections.Generic;

namespace megamarket.Models;

public partial class Productstatus
{
    public int Id { get; set; }

    public string Statusname { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
