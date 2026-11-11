using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using megamarket.Models;
using Microsoft.EntityFrameworkCore;

namespace megamarket.Services;

public class OrderService
{
    private readonly MegamarketContext _context;

    public OrderService(MegamarketContext context)
    {
        _context = context;
    }

    public async Task<Address> CreateAddressAsync(string city, string street, string house, string apartment)
    {
        var address = new Address
        {
            Region = city,
            City = city,
            Street = street,
            House = house,
            Apartment = apartment,
            Postcode = "000000"
        };
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
        return address;
    }

    public async Task<int> CreateOrderAsync(int userId, int addressId, List<CartItem> items, decimal totalPrice, decimal deliveryCost, decimal usedBonuses)
    {
        var customerProfile = await _context.Customerprofiles
            .FirstOrDefaultAsync(c => c.Userid == userId);
        
        if (customerProfile == null)
            throw new Exception("Профиль покупателя не найден!");

        int customerProfileId = customerProfile.Id;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                Customerid = customerProfileId, 
                Deliveryaddressid = addressId,
                Totalprice = totalPrice,
                Deliverycost = deliveryCost,
                Statusid = 2, // "Paid"
                Orderdate = DateTime.UtcNow
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                var orderItem = new Orderitem
                {
                    Orderid = order.Id,
                    Productid = item.Product.Id,
                    Quantity = item.Quantity,
                    Priceatpurchase = item.Product.Baseprice,
                    Sellerid = item.Product.Sellerid 
                };
                _context.Orderitems.Add(orderItem);

                var stock = await _context.Productstocks
                    .FirstOrDefaultAsync(ps => ps.Productid == item.Product.Id);
                
                if (stock != null)
                {
                    stock.Quantityinstock -= item.Quantity;
                    if (stock.Quantityinstock < 0) stock.Quantityinstock = 0;
                }
            }
            await _context.SaveChangesAsync();

            _context.Orderstatuses.Add(new Orderstatus
            {
                Orderid = order.Id,
                Status = "Created",
                Changedat = DateTime.UtcNow
            });
            
            if (usedBonuses > 0)
            {
                customerProfile.Bonuspointsbalance -= usedBonuses;
            }

            _context.Transactions.Add(new Transaction
            {
                Orderid = order.Id,
                Userid = userId,
                Amount = totalPrice,
                Type = "Payment",
                Transactiondate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return order.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw new Exception(realError);
        }
    }

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        var customerProfile = await _context.Customerprofiles.FirstOrDefaultAsync(c => c.Userid == userId);
        if (customerProfile == null) return new List<Order>();

        return await _context.Orders
            .Include(o => o.Orderitems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.Customerid == customerProfile.Id) // 👈 Убрали фильтр по Ishidden
            .OrderBy(o => o.Statusid == 6 ? 1 : 0) 
            .ThenByDescending(o => o.Orderdate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetSellerOrdersAsync(int sellerId)
    {
        return await _context.Orders
            .Include(o => o.Orderitems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.Orderitems.Any(oi => oi.Sellerid == sellerId))
            .OrderByDescending(o => o.Orderdate)
            .ToListAsync();
    }

    public async Task UpdateOrderStatusAsync(int orderId, int newStatusId, string statusName)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Statusid = newStatusId;
            
            _context.Orderstatuses.Add(new Orderstatus
            {
                Orderid = orderId,
                Status = statusName,
                Changedat = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders
                .Include(o => o.Orderitems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            order.Statusid = 6; // Отменен

            _context.Orderstatuses.Add(new Orderstatus
            {
                Orderid = order.Id,
                Status = "Cancelled",
                Changedat = DateTime.UtcNow
            });

            foreach (var item in order.Orderitems)
            {
                var stock = await _context.Productstocks
                    .FirstOrDefaultAsync(ps => ps.Productid == item.Productid);
                
                if (stock != null)
                {
                    stock.Quantityinstock += item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}