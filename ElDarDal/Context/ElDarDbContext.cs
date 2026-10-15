using ElDarDal.Models;
using Microsoft.EntityFrameworkCore;

namespace ElDarDal.Context
{
    public class ElDarDbContext : DbContext
    {
        public ElDarDbContext(DbContextOptions<ElDarDbContext> options) : base(options)
        {
        }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Data> Data { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<DeletedLogs> DeletedLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .Property(e=>e.Total)
                .HasComputedColumnSql("[Salary] + COALESCE([Adds], 0) - COALESCE([Taxs], 0)", stored: true);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Job)
                .HasConversion<string>();

            modelBuilder.Entity<Guest>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Data>()
                .Property(e => e.Revenues)
                .HasConversion<string>();

            modelBuilder.Entity<Data>()
                .Property(e => e.Payments)
                .HasConversion<string>();

        }
    }
}
