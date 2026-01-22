using Microsoft.EntityFrameworkCore;
using PaymentService.WebApi.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PaymentService.WebApi.Infrastructure;

public sealed class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(b =>
        {
            b.ToTable("payments");
            b.HasKey(x => x.OrderId);

            // OrderId приходит извне (из order-service), автоинкремента нет
            b.Property(x => x.OrderId).ValueGeneratedNever();

            b.Property(x => x.Price).HasColumnType("numeric(18,2)").IsRequired();
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.DateCreate).IsRequired();
        });
    }
}