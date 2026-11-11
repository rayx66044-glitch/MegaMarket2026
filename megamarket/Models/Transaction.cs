using System;
using System.Collections.Generic;

namespace megamarket.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public int? Orderid { get; set; }

    public int Userid { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = null!;

    public DateTime Transactiondate { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User User { get; set; } = null!;
}
