using DotnetBilling.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetBilling.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.Address).HasMaxLength(250);
            entity.Property(x => x.TaxNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.TaxRate).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.InvoiceNumber).HasMaxLength(30).IsRequired();
            entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 2);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.TaxRate).HasPrecision(5, 2);
            entity.Property(x => x.LineSubtotal).HasPrecision(18, 2);
            entity.Property(x => x.LineTaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);

            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.InvoiceItems)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaymentMethod).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ReferenceNumber).HasMaxLength(100);

            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
