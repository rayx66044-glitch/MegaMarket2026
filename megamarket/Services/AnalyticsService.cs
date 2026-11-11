using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using megamarket.Models;
using Microsoft.EntityFrameworkCore;

namespace megamarket.Services;

public class AnalyticsService
{
    private readonly MegamarketContext _context;

    public AnalyticsService(MegamarketContext context)
    {
        _context = context;
    }

    // Выручка по дням за последние 7 дней
    public async Task<Dictionary<string, double>> GetSalesByDayAsync(int sellerId)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;

        var sales = await _context.Orderitems
            .Include(oi => oi.Order)
            .Where(oi => oi.Sellerid == sellerId && oi.Order.Orderdate >= sevenDaysAgo)
            .Select(oi => new { oi.Order.Orderdate, oi.Priceatpurchase, oi.Quantity })
            .ToListAsync();

        var result = new Dictionary<string, double>();

        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddDays(-i);
            var daySales = sales.Where(s => s.Orderdate.Date == date.Date)
                                .Sum(s => (double)(s.Priceatpurchase * s.Quantity));
            result[date.ToString("dd.MM")] = daySales;
        }

        return result;
    }

    // Топ-5 популярных товаров (с обрезкой длинных названий)
    public async Task<Dictionary<string, int>> GetTopProductsAsync(int sellerId)
    {
        var topProducts = await _context.Orderitems
            .Include(oi => oi.Product)
            .Where(oi => oi.Sellerid == sellerId)
            .GroupBy(oi => oi.Product.Name)
            .Select(g => new { Name = g.Key, Count = g.Sum(oi => oi.Quantity) })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToDictionaryAsync(g => g.Name, g => g.Count);

        // Обрезаем названия, чтобы они помещались на графике
        var result = new Dictionary<string, int>();
        foreach (var item in topProducts)
        {
            string shortName = item.Key.Length > 15 ? item.Key.Substring(0, 15) + "..." : item.Key;
            result[shortName] = item.Value;
        }
        return result;
    }

    // Общая статистика
    public async Task<(decimal Revenue, int OrdersCount)> GetTotalStatsAsync(int sellerId)
    {
        var items = await _context.Orderitems
            .Where(oi => oi.Sellerid == sellerId)
            .ToListAsync();

        var revenue = items.Sum(i => i.Priceatpurchase * i.Quantity);
        var ordersCount = items.Select(i => i.Orderid).Distinct().Count();

        return (revenue, ordersCount);
    }
}