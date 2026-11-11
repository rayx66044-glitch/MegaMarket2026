using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using megamarket.Models;

namespace megamarket.Services;

public partial class CartItem : ObservableObject
{
    public Product Product { get; set; } = null!;
    
    [ObservableProperty]
    private int _quantity;

    public int MaxStock { get; set; }

    public decimal TotalPrice => Product.Baseprice * Quantity;

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(TotalPrice));
    }
}

public class CartService
{
    public List<CartItem> Items { get; } = new();

    public void AddToCart(Product product, int quantity = 1)
    {
        var existing = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existing != null)
        {
            if (existing.Quantity + quantity <= existing.MaxStock)
                existing.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItem 
            { 
                Product = product, 
                Quantity = quantity,
                MaxStock = product.CurrentStock 
            });
        }
    }

    public void RemoveFromCart(int productId)
    {
        var item = Items.FirstOrDefault(i => i.Product.Id == productId);
        if (item != null) Items.Remove(item);
    }

    public void UpdateQuantity(int productId, int newQuantity)
    {
        var item = Items.FirstOrDefault(i => i.Product.Id == productId);
        if (item != null)
        {
            if (newQuantity <= 0)
                Items.Remove(item);
            else if (newQuantity <= item.MaxStock)
                item.Quantity = newQuantity;
        }
    }

    public void Clear() => Items.Clear();

    // 👇 ЭТОГО МЕТОДА НЕ ХВАТАЛ В ВАШЕМ КОДЕ
    public decimal GetTotalPrice()
    {
        return Items.Sum(item => item.TotalPrice);
    }
}