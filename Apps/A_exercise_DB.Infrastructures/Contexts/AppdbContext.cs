using A_exercise_DB.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore;

namespace A_exercise_DB.Infrastructures.Contexts;

/// <summary>
/// アプリケーション用データベースコンテキスト
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<ProductStockEntity> ProductStocks => Set<ProductStockEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<ProductCategoryEntity> ProductCategories => Set<ProductCategoryEntity>();
    public DbSet<PaymentMethodEntity> PaymentMethods => Set<PaymentMethodEntity>();
    public DbSet<OrderStatusEntity> OrderStatuses => Set<OrderStatusEntity>();
    public DbSet<OrdersEntity> Orders => Set<OrdersEntity>();
    public DbSet<OrdersDetailEntity> OrdersDetails => Set<OrdersDetailEntity>();
    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();
    public DbSet<EmployeeAccountEntity> EmployeeAccounts => Set<EmployeeAccountEntity>();
    public DbSet<DepartmentEntity> Departments => Set<DepartmentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 顧客
        modelBuilder.Entity<CustomerEntity>(e =>
        {
            e.HasIndex(c => c.CustomerUuid).IsUnique();
            e.HasIndex(c => c.Username).IsUnique();
            e.HasIndex(c => c.MailAddress).IsUnique();

            e.Property(c => c.Name).HasMaxLength(50);
            e.Property(c => c.Address1).HasMaxLength(100);
            e.Property(c => c.Address2).HasMaxLength(100);
            e.Property(c => c.PhoneNumber).HasMaxLength(20);
            e.Property(c => c.MailAddress).HasMaxLength(100);
            e.Property(c => c.Username).HasMaxLength(50);
            e.Property(c => c.Password).HasMaxLength(100);
        });

        // 商品カテゴリ
        modelBuilder.Entity<ProductCategoryEntity>(e =>
        {
            e.HasIndex(c => c.CategoryUuid).IsUnique();
            e.Property(c => c.Name).HasMaxLength(20);

        });

        // 商品
        modelBuilder.Entity<ProductEntity>(e =>
        {
            e.HasIndex(p => p.ProductUuid).IsUnique();

            e.Property(p => p.Name).HasMaxLength(50);
            e.Property(p => p.ImageUrl).HasMaxLength(255);

            e.HasOne(p => p.ProductCategory)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.ProductCategoryId)
                .HasConstraintName("product_ibfk_category")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.ProductStock)
                .WithOne(s => s.Product)
                .HasForeignKey<ProductStockEntity>(s => s.ProductId)
                .HasConstraintName("product_stock_ibfk_product")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 商品在庫
        modelBuilder.Entity<ProductStockEntity>(e =>
        {
            e.HasIndex(s => s.StockUuid).IsUnique();
            e.HasIndex(s => s.ProductId).IsUnique();
        });

        // 支払い方法
        modelBuilder.Entity<PaymentMethodEntity>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(30);
        });

        // 注文ステータス
        modelBuilder.Entity<OrderStatusEntity>(e =>
        {
            e.Property(o => o.Name).HasMaxLength(30);
        });

        // 注文
        modelBuilder.Entity<OrdersEntity>(e =>
        {
            e.HasIndex(o => o.OrderUuid).IsUnique();

            e.HasOne(o => o.Customer)
                .WithMany(c => c.ListOrders)
                .HasForeignKey(o => o.CustomerId)
                .HasConstraintName("orders_ibfk_customer")
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.OrderStatus)
                .WithMany(s => s.ListOrders)
                .HasForeignKey(o => o.OrderStatusId)
                .HasConstraintName("orders_ibfk_order_status")
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.PaymentMethod)
                .WithMany(p => p.ListOrders)
                .HasForeignKey(o => o.PaymentMethodId)
                .HasConstraintName("orders_ibfk_payment_method")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 注文明細
        modelBuilder.Entity<OrdersDetailEntity>(e =>
        {
            e.HasOne(d => d.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("orders_detail_ibfk_order")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("orders_detail_ibfk_product")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 部署
        modelBuilder.Entity<DepartmentEntity>(e =>
        {
            e.HasIndex(d => d.DepartmentUuid).IsUnique();
            e.Property(d => d.Name).HasMaxLength(30);
        });

        // 社員
        modelBuilder.Entity<EmployeeEntity>(e =>
        {
            e.HasIndex(emp => emp.EmployeeUuid).IsUnique();

            e.Property(emp => emp.Name).HasMaxLength(50);
            e.Property(emp => emp.Kana).HasMaxLength(50);

            e.HasOne(emp => emp.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(emp => emp.DepartmentId)
                .HasConstraintName("employee_ibfk_department")
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(emp => emp.EmployeeAccount)
                .WithOne(a => a.Employee)
                .HasForeignKey<EmployeeAccountEntity>(a => a.EmployeeId)
                .HasConstraintName("employee_account_ibfk_employee")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 社員アカウント
        modelBuilder.Entity<EmployeeAccountEntity>(e =>
        {
            e.HasIndex(a => a.AccountUuid).IsUnique();
            e.HasIndex(a => a.Name).IsUnique();
            e.HasIndex(a => a.EmployeeId).IsUnique();

            e.Property(a => a.Name).HasMaxLength(50);
            e.Property(a => a.Password).HasMaxLength(100);
        });
    }
}