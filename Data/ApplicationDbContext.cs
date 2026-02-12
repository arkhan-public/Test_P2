using Microsoft.EntityFrameworkCore;
using InventorySystem.Models;

namespace InventorySystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.Product)
                .WithMany(p => p.StockTransactions)
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(poi => poi.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.Product)
                .WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(poi => poi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrderItem>()
                .HasOne(soi => soi.SalesOrder)
                .WithMany(so => so.Items)
                .HasForeignKey(soi => soi.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesOrderItem>()
                .HasOne(soi => soi.Product)
                .WithMany(p => p.SalesOrderItems)
                .HasForeignKey(soi => soi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories", CreatedAt = DateTime.Now },
                new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items", CreatedAt = DateTime.Now },
                new Category { Id = 3, Name = "Office Supplies", Description = "Office equipment and stationery", CreatedAt = DateTime.Now },
                new Category { Id = 4, Name = "Home Appliances", Description = "Household appliances and equipment", CreatedAt = DateTime.Now }
            );

            // Seed Suppliers
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier
                {
                    Id = 1,
                    Name = "Tech Solutions Inc.",
                    Email = "contact@techsolutions.com",
                    Phone = "+1-555-0101",
                    Address = "123 Tech Street, Silicon Valley, CA 94025",
                    CreatedAt = DateTime.Now
                },
                new Supplier
                {
                    Id = 2,
                    Name = "Global Traders Ltd.",
                    Email = "info@globaltraders.com",
                    Phone = "+1-555-0102",
                    Address = "456 Commerce Ave, New York, NY 10001",
                    CreatedAt = DateTime.Now
                },
                new Supplier
                {
                    Id = 3,
                    Name = "Quality Suppliers Co.",
                    Email = "sales@qualitysuppliers.com",
                    Phone = "+1-555-0103",
                    Address = "789 Business Blvd, Chicago, IL 60601",
                    CreatedAt = DateTime.Now
                }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Wireless Mouse",
                    SKU = "ELEC-001",
                    Description = "Ergonomic wireless mouse with USB receiver",
                    UnitPrice = 29.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 15,
                    CategoryId = 1,
                    SupplierId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 2,
                    Name = "Mechanical Keyboard",
                    SKU = "ELEC-002",
                    Description = "RGB mechanical gaming keyboard",
                    UnitPrice = 89.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 10,
                    CategoryId = 1,
                    SupplierId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 3,
                    Name = "USB-C Hub",
                    SKU = "ELEC-003",
                    Description = "7-in-1 USB-C multiport adapter",
                    UnitPrice = 49.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 20,
                    CategoryId = 1,
                    SupplierId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1625948515291-69613efd103f?w=400",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 4,
                    Name = "Cotton T-Shirt",
                    SKU = "CLTH-001",
                    Description = "100% cotton crew neck t-shirt",
                    UnitPrice = 19.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 30,
                    CategoryId = 2,
                    SupplierId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 5,
                    Name = "Denim Jeans",
                    SKU = "CLTH-002",
                    Description = "Classic fit denim jeans",
                    UnitPrice = 59.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 25,
                    CategoryId = 2,
                    SupplierId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1542272604-787c3835535d?w=400",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 6,
                    Name = "Office Desk Lamp",
                    SKU = "OFF-001",
                    Description = "LED desk lamp with adjustable brightness",
                    UnitPrice = 39.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 15,
                    CategoryId = 3,
                    SupplierId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=400",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 7,
                    Name = "Notebook Set",
                    SKU = "OFF-002",
                    Description = "Pack of 5 ruled notebooks",
                    UnitPrice = 12.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 40,
                    CategoryId = 3,
                    SupplierId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1517842645767-c639042777db?w=400",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 8,
                    Name = "Ballpoint Pens",
                    SKU = "OFF-003",
                    Description = "Box of 50 blue ballpoint pens",
                    UnitPrice = 15.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 50,
                    CategoryId = 3,
                    SupplierId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1586951728566-d1293c2e8c7d?w=400",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 9,
                    Name = "Coffee Maker",
                    SKU = "HOME-001",
                    Description = "12-cup programmable coffee maker",
                    UnitPrice = 79.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 8,
                    CategoryId = 4,
                    SupplierId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=400",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 10,
                    Name = "Vacuum Cleaner",
                    SKU = "HOME-002",
                    Description = "Cordless stick vacuum cleaner",
                    UnitPrice = 199.99m,
                    QuantityInStock = 0,
                    MinimumStockLevel = 5,
                    CategoryId = 4,
                    SupplierId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1558317374-067fb5f30001?w=400",
                    CreatedAt = DateTime.Now
                }
            );
        }
    }
}
