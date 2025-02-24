using Microsoft.EntityFrameworkCore;
using AmoElDiseno.Models;

namespace AmoElDiseno.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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
        
        public DbSet<AmoElDiseno.Models.Customer> Customer { get; set; } = default!;
        public DbSet<AmoElDiseno.Models.Expense> Expense { get; set; } = default!;
        public DbSet<AmoElDiseno.Models.Income> Income { get; set; } = default!;
        public DbSet<AmoElDiseno.Models.Order> Order { get; set; } = default!;
        public DbSet<AmoElDiseno.Models.User> User { get; set; } = default!;

    }
}
