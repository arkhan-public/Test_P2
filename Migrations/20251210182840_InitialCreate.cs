using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventorySystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityInStock = table.Column<int>(type: "int", nullable: false),
                    MinimumStockLevel = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderItems_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    BalanceAfter = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8565), "Electronic devices and accessories", "Electronics" },
                    { 2, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8568), "Apparel and fashion items", "Clothing" },
                    { 3, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8569), "Office equipment and stationery", "Office Supplies" },
                    { 4, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8601), "Household appliances and equipment", "Home Appliances" }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "Name", "Phone" },
                values: new object[,]
                {
                    { 1, "123 Tech Street, Silicon Valley, CA 94025", new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8859), "contact@techsolutions.com", "Tech Solutions Inc.", "+1-555-0101" },
                    { 2, "456 Commerce Ave, New York, NY 10001", new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8860), "info@globaltraders.com", "Global Traders Ltd.", "+1-555-0102" },
                    { 3, "789 Business Blvd, Chicago, IL 60601", new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8861), "sales@qualitysuppliers.com", "Quality Suppliers Co.", "+1-555-0103" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrl", "MinimumStockLevel", "Name", "QuantityInStock", "SKU", "SupplierId", "UnitPrice", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8892), "Ergonomic wireless mouse with USB receiver", "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400", 15, "Wireless Mouse", 0, "ELEC-001", 1, 29.99m, null },
                    { 2, 1, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8894), "RGB mechanical gaming keyboard", "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400", 10, "Mechanical Keyboard", 0, "ELEC-002", 1, 89.99m, null },
                    { 3, 1, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8896), "7-in-1 USB-C multiport adapter", "https://images.unsplash.com/photo-1625948515291-69613efd103f?w=400", 20, "USB-C Hub", 0, "ELEC-003", 1, 49.99m, null },
                    { 4, 2, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8898), "100% cotton crew neck t-shirt", "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400", 30, "Cotton T-Shirt", 0, "CLTH-001", 2, 19.99m, null },
                    { 5, 2, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8900), "Classic fit denim jeans", "https://images.unsplash.com/photo-1542272604-787c3835535d?w=400", 25, "Denim Jeans", 0, "CLTH-002", 2, 59.99m, null },
                    { 6, 3, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8902), "LED desk lamp with adjustable brightness", "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=400", 15, "Office Desk Lamp", 0, "OFF-001", 3, 39.99m, null },
                    { 7, 3, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8904), "Pack of 5 ruled notebooks", "https://images.unsplash.com/photo-1517842645767-c639042777db?w=400", 40, "Notebook Set", 0, "OFF-002", 3, 12.99m, null },
                    { 8, 3, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8905), "Box of 50 blue ballpoint pens", "https://images.unsplash.com/photo-1586951728566-d1293c2e8c7d?w=400", 50, "Ballpoint Pens", 0, "OFF-003", 3, 15.99m, null },
                    { 9, 4, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8907), "12-cup programmable coffee maker", "https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=400", 8, "Coffee Maker", 0, "HOME-001", 1, 79.99m, null },
                    { 10, 4, new DateTime(2025, 12, 11, 0, 28, 38, 805, DateTimeKind.Local).AddTicks(8909), "Cordless stick vacuum cleaner", "https://images.unsplash.com/photo-1558317374-067fb5f30001?w=400", 5, "Vacuum Cleaner", 0, "HOME-002", 2, 199.99m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_ProductId",
                table: "SalesOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_SalesOrderId",
                table: "SalesOrderItems",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ProductId",
                table: "StockTransactions",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "SalesOrderItems");

            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
