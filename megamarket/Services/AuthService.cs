using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using megamarket.Models;
using Microsoft.EntityFrameworkCore;

namespace megamarket.Services;

public class AuthService
{
    private readonly MegamarketContext _context;

    public AuthService(MegamarketContext context)
    {
        _context = context;
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        var hash = ComputeHash(password, user.Salt);
        return user.Passwordhash == hash ? user : null;
    }

    // Измените сигнатуру метода, добавив firstName и lastName
    public async Task<bool> RegisterAsync(string email, string password, string phone, string? inn, bool isSeller, string firstName = "Имя", string lastName = "Фамилия")
    {
        if (await _context.Users.AnyAsync(u => u.Email == email)) return false;

        var salt = Guid.NewGuid().ToString("N");
        var hash = ComputeHash(password, salt);

        var user = new User
        {
            Email = email,
            Phone = phone,
            Passwordhash = hash,
            Salt = salt,
            Roleid = isSeller ? 3 : 2, // 👈 ИСПРАВЛЕНО: 3 - Продавец, 2 - Покупатель!
            Createdat = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (isSeller)
        {
            _context.Sellerprofiles.Add(new Sellerprofile
            {
                Userid = user.Id,
                Companyname = string.IsNullOrWhiteSpace(firstName) ? "Новый продавец" : firstName, 
                Inn = inn ?? "",
                Balance = 0,
                Rating = 0
            });
        }
        else
        {
            _context.Customerprofiles.Add(new Customerprofile
            {
                Userid = user.Id,
                Firstname = string.IsNullOrWhiteSpace(firstName) ? "Имя" : firstName,
                Lastname = string.IsNullOrWhiteSpace(lastName) ? "Фамилия" : lastName,
                Bonuspointsbalance = 500 // Дадим новичкам 500 бонусов для теста!
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }
    private string ComputeHash(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + salt);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}