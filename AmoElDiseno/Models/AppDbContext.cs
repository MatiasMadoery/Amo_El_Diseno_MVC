using Microsoft.EntityFrameworkCore;
using AmoElDiseno.Models;

namespace AmoElDiseno.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public DbSet<Customer> Customers { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<PaymentDelivery> PaymentDeliveries { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relación entre Customer y Order
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId);

            // Conversión de enum OrderStatus a string
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            // Relación entre Order y PaymentDelivery
            modelBuilder.Entity<Order>()
                .HasMany(o => o.PaymentDeliveries)
                .WithOne(pd => pd.Order)
                .HasForeignKey(pd => pd.OrderId);
            
        }

    }
}
