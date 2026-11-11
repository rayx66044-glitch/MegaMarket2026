using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace megamarket.Models;

public partial class MegamarketContext : DbContext
{
    public MegamarketContext()
    {
    }

    public MegamarketContext(DbContextOptions<MegamarketContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customerprofile> Customerprofiles { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderitem> Orderitems { get; set; }

    public virtual DbSet<Orderstatus> Orderstatuses { get; set; }

    public virtual DbSet<Orderstatusesdict> Orderstatusesdicts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Productcharacteristic> Productcharacteristics { get; set; }

    public virtual DbSet<Productstatus> Productstatuses { get; set; }

    public virtual DbSet<Productstock> Productstocks { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sellerprofile> Sellerprofiles { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;UserName=postgres;Password=root;DataBase=megamarket");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>().Ignore(p => p.CurrentStock);
        // Отключаем строгую проверку UTC для Postgres (решает 90% ошибок сохранения)
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("addresses_pkey");

            entity.ToTable("addresses");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Apartment)
                .HasMaxLength(20)
                .HasColumnName("apartment");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.House)
                .HasMaxLength(20)
                .HasColumnName("house");
            entity.Property(e => e.Postcode)
                .HasMaxLength(10)
                .HasColumnName("postcode");
            entity.Property(e => e.Region)
                .HasMaxLength(100)
                .HasColumnName("region");
            entity.Property(e => e.Street)
                .HasMaxLength(150)
                .HasColumnName("street");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Parentcategoryid).HasColumnName("parentcategoryid");

            entity.HasOne(d => d.Parentcategory).WithMany(p => p.InverseParentcategory)
                .HasForeignKey(d => d.Parentcategoryid)
                .HasConstraintName("fk_categories_parent");
        });

        modelBuilder.Entity<Customerprofile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customerprofiles_pkey");

            entity.ToTable("customerprofiles");

            entity.HasIndex(e => e.Userid, "customerprofiles_userid_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Bonuspointsbalance)
                .HasPrecision(18, 2)
                .HasColumnName("bonuspointsbalance");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("lastname");
            entity.Property(e => e.Mainaddressid).HasColumnName("mainaddressid");
            entity.Property(e => e.Middlename)
                .HasMaxLength(100)
                .HasColumnName("middlename");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Mainaddress).WithMany(p => p.Customerprofiles)
                .HasForeignKey(d => d.Mainaddressid)
                .HasConstraintName("fk_customerprofiles_addresses");

            entity.HasOne(d => d.User).WithOne(p => p.Customerprofile)
                .HasForeignKey<Customerprofile>(d => d.Userid)
                .HasConstraintName("fk_customerprofiles_users");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.HasIndex(e => e.Customerid, "ix_orders_customerid");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Customerid).HasColumnName("customerid");
            entity.Property(e => e.Deliveryaddressid).HasColumnName("deliveryaddressid");
            entity.Property(e => e.Deliverycost)
                .HasPrecision(18, 2)
                .HasColumnName("deliverycost");
            entity.Property(e => e.Orderdate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("orderdate");
            entity.Property(e => e.Statusid).HasColumnName("statusid");
            entity.Property(e => e.Totalprice)
                .HasPrecision(18, 2)
                .HasColumnName("totalprice");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orders_customerprofiles");

            entity.HasOne(d => d.Deliveryaddress).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Deliveryaddressid)
                .HasConstraintName("fk_orders_addresses");

            entity.HasOne(d => d.Status).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Statusid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orders_statuses");
        });

        modelBuilder.Entity<Orderitem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orderitems_pkey");

            entity.ToTable("orderitems");

            entity.HasIndex(e => e.Orderid, "ix_orderitems_orderid");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Priceatpurchase)
                .HasPrecision(18, 2)
                .HasColumnName("priceatpurchase");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Sellerid).HasColumnName("sellerid");

            entity.HasOne(d => d.Order).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("fk_orderitems_orders");

            entity.HasOne(d => d.Product).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Productid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orderitems_products");

            entity.HasOne(d => d.Seller).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Sellerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orderitems_sellerprofiles");
        });

        modelBuilder.Entity<Orderstatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orderstatuses_pkey");

            entity.ToTable("orderstatuses");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Changedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("changedat");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Order).WithMany(p => p.Orderstatuses)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("fk_orderstatuses_orders");
        });

        modelBuilder.Entity<Orderstatusesdict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orderstatusesdict_pkey");

            entity.ToTable("orderstatusesdict");

            entity.HasIndex(e => e.Statusname, "orderstatusesdict_statusname_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Statusname)
                .HasMaxLength(50)
                .HasColumnName("statusname");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products");

            entity.HasIndex(e => e.Categoryid, "ix_products_categoryid");

            entity.HasIndex(e => e.Sellerid, "ix_products_sellerid");

            entity.HasIndex(e => e.Sku, "products_sku_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Barcode)
                .HasMaxLength(50)
                .HasColumnName("barcode");
            entity.Property(e => e.Baseprice)
                .HasPrecision(18, 2)
                .HasColumnName("baseprice");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Currentdiscount)
                .HasPrecision(5, 2)
                .HasColumnName("currentdiscount");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.Sellerid).HasColumnName("sellerid");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("sku");
            entity.Property(e => e.Statusid).HasColumnName("statusid");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_products_categories");

            entity.HasOne(d => d.Seller).WithMany(p => p.Products)
                .HasForeignKey(d => d.Sellerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_products_sellerprofiles");

            entity.HasOne(d => d.Status).WithMany(p => p.Products)
                .HasForeignKey(d => d.Statusid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_products_statuses");
        });

        modelBuilder.Entity<Productcharacteristic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productcharacteristics_pkey");

            entity.ToTable("productcharacteristics");

            entity.HasIndex(e => e.Productid, "ix_productcharacteristics_productid");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Keyname)
                .HasMaxLength(100)
                .HasColumnName("keyname");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Value)
                .HasMaxLength(250)
                .HasColumnName("value");

            entity.HasOne(d => d.Product).WithMany(p => p.Productcharacteristics)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("fk_productcharacteristics_products");
        });

        modelBuilder.Entity<Productstatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productstatuses_pkey");

            entity.ToTable("productstatuses");

            entity.HasIndex(e => e.Statusname, "productstatuses_statusname_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Statusname)
                .HasMaxLength(50)
                .HasColumnName("statusname");
        });

        modelBuilder.Entity<Productstock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productstocks_pkey");

            entity.ToTable("productstocks");

            entity.HasIndex(e => new { e.Productid, e.Warehouseid }, "uq_productstocks").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Quantityinstock)
                .HasDefaultValue(0)
                .HasColumnName("quantityinstock");
            entity.Property(e => e.Reservedquantity)
                .HasDefaultValue(0)
                .HasColumnName("reservedquantity");
            entity.Property(e => e.Warehouseid).HasColumnName("warehouseid");

            entity.HasOne(d => d.Product).WithMany(p => p.Productstocks)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("fk_productstocks_products");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Productstocks)
                .HasForeignKey(d => d.Warehouseid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_productstocks_warehouses");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reviews_pkey");

            entity.ToTable("reviews");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Customerid).HasColumnName("customerid");
            entity.Property(e => e.Imageurls).HasColumnName("imageurls");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Textreview).HasColumnName("textreview");

            entity.HasOne(d => d.Customer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_customerprofiles");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("fk_reviews_products");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Rolename, "roles_rolename_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Permissionsmask)
                .HasDefaultValue(0)
                .HasColumnName("permissionsmask");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Sellerprofile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sellerprofiles_pkey");

            entity.ToTable("sellerprofiles");

            entity.HasIndex(e => e.Userid, "sellerprofiles_userid_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasPrecision(18, 2)
                .HasColumnName("balance");
            entity.Property(e => e.Companyname)
                .HasMaxLength(200)
                .HasColumnName("companyname");
            entity.Property(e => e.Inn)
                .HasMaxLength(12)
                .HasColumnName("inn");
            entity.Property(e => e.Kpp)
                .HasMaxLength(9)
                .HasColumnName("kpp");
            entity.Property(e => e.Legaladdressid).HasColumnName("legaladdressid");
            entity.Property(e => e.Rating)
                .HasPrecision(3, 2)
                .HasColumnName("rating");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Legaladdress).WithMany(p => p.Sellerprofiles)
                .HasForeignKey(d => d.Legaladdressid)
                .HasConstraintName("fk_sellerprofiles_addresses");

            entity.HasOne(d => d.User).WithOne(p => p.Sellerprofile)
                .HasForeignKey<Sellerprofile>(d => d.Userid)
                .HasConstraintName("fk_sellerprofiles_users");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transactions_pkey");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.Orderid, "ix_transactions_orderid");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Transactiondate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transactiondate");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("fk_transactions_orders");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_transactions_users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Roleid, "ix_users_roleid");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Isbanned)
                .HasDefaultValue(false)
                .HasColumnName("isbanned");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(256)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Salt)
                .HasMaxLength(64)
                .HasColumnName("salt");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_roles");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouses_pkey");

            entity.ToTable("warehouses");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(250)
                .HasColumnName("address");
            entity.Property(e => e.Capacity)
                .HasDefaultValue(0)
                .HasColumnName("capacity");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Warehousename)
                .HasMaxLength(150)
                .HasColumnName("warehousename");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
