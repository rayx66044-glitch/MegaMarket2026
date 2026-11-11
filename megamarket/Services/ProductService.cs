using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using megamarket.Models;
using Microsoft.EntityFrameworkCore;

namespace megamarket.Services;

public class ProductService
{
    private readonly MegamarketContext _context;

    public ProductService(MegamarketContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.Parentcategoryid == null && c.Isactive == true)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsAsync(
        int? categoryId, 
        string? searchQuery, 
        decimal? minPrice, 
        decimal? maxPrice, 
        bool inStockOnly, 
        int skip, 
        int take)
    {
        var query = _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Productstocks)
            .Where(p => p.Statusid == 3);

        // Фильтр по категории (включая подкатегории)
        if (categoryId.HasValue)
        {
            var subCategoryIds = await _context.Categories
                .Where(c => c.Id == categoryId.Value || c.Parentcategoryid == categoryId.Value)
                .Select(c => c.Id)
                .ToListAsync();

            query = query.Where(p => subCategoryIds.Contains(p.Categoryid));
        }

        // Поиск по названию
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var lowerSearch = searchQuery.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(lowerSearch));
        }

        // Фильтр по цене
        if (minPrice.HasValue)
            query = query.Where(p => p.Baseprice >= minPrice.Value);
        
        if (maxPrice.HasValue)
            query = query.Where(p => p.Baseprice <= maxPrice.Value);

        // Фильтр "Только в наличии"
        if (inStockOnly)
        {
            query = query.Where(p => p.Productstocks.Any(ps => ps.Quantityinstock > 0));
        }

        return await query
            .OrderByDescending(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Product?> GetProductDetailsAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Productcharacteristics)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
}