using System;
using System.Collections.Generic;

namespace megamarket.Models;

public partial class Productcharacteristic
{
    public int Id { get; set; }

    public int Productid { get; set; }

    public string Keyname { get; set; } = null!;

    public string? Value { get; set; }

    public virtual Product Product { get; set; } = null!;
}
