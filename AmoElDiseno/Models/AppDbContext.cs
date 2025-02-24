using Microsoft.EntityFrameworkCore;

namespace AmoElDiseno.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public class BusinessContext : DbContext
        {
            public DbSet<Customer>? Customers { get; set; }
            public DbSet<Order>? Orders { get; set; }
            public DbSet<Income>? Incomes { get; set; }
            public DbSet<Expense>? Expenses { get; set; }
            public DbSet<User>? Users { get; set; }           

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>()
                    .HasMany(c => c.Orders)
                    .WithOne(o => o.Customer)
                    .HasForeignKey(o => o.CustomerId);

                modelBuilder.Entity<Order>()
                    .Property(o => o.Status)
                    .HasConversion<string>();
            }
        }

    }
}
