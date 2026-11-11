using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using megamarket.Models;
using Microsoft.EntityFrameworkCore;

namespace megamarket.Services;

public class WarehouseService
{
    private readonly MegamarketContext _context;

    public WarehouseService(MegamarketContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetSellerProductsAsync(int sellerId)
    {
        return await _context.Products
            .Include(p => p.Productstocks) // Проверьте: Productstocks или ProductStocks?
            .Where(p => p.Sellerid == sellerId) // Проверьте: Sellerid или SellerId?
            .ToListAsync();
    }

    public async Task CreateProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task SaveStocksAsync()
    {
        // Сохраняем изменения, сделанные в DataGrid
        await _context.SaveChangesAsync();
    }
}