namespace Api.Data;

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    public DbSet<SampleProduct> Products { get; set; }
    public DbSet<SampleOrder> Orders { get; set; }
    public DbSet<SampleCompoundKeyOrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite key for OrderDetail
        modelBuilder.Entity<SampleCompoundKeyOrderDetail>()
            .HasKey(od => new { od.OrderId, od.ProductId });

        // Configure relationships
        modelBuilder.Entity<SampleCompoundKeyOrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SampleCompoundKeyOrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data
        modelBuilder.Entity<SampleProduct>().HasData(
            new SampleProduct
            {
                ProductId = 1,
                Name = "Sample Product 1",
                Price = 19.99M,
                Description = "This is a sample product"
            },
            new SampleProduct
            {
                ProductId = 2,
                Name = "Sample Product 2",
                Price = 29.99M,
                Description = "This is another sample product"
            }
        );

        modelBuilder.Entity<SampleOrder>().HasData(
            new SampleOrder
            {
                OrderId = 1,
                OrderDate = DateTime.UtcNow,
                CustomerName = "John Doe",
                TotalAmount = 49.98M
            }
        );

        modelBuilder.Entity<SampleCompoundKeyOrderDetail>().HasData(
            new SampleCompoundKeyOrderDetail
            {
                OrderId = 1,
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 19.99M
            },
            new SampleCompoundKeyOrderDetail
            {
                OrderId = 1,
                ProductId = 2,
                Quantity = 1,
                UnitPrice = 29.99M
            }
        );
    }
}
