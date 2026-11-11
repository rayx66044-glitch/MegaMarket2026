using System.Linq;

namespace megamarket.Models;

public partial class Product
{
    public int CurrentStock 
    {
        get 
        {
            if (Productstocks != null && Productstocks.Any())
            {
                return Productstocks.First().Quantityinstock;
            }
            return 0;
        }
        set 
        {
            if (Productstocks != null && Productstocks.Any())
            {
                Productstocks.First().Quantityinstock = value;
            }
        }
    }
}