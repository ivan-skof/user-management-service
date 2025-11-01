using Microsoft.EntityFrameworkCore;
using UserManagementService.Data.Entities;

namespace UserManagementService.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ApiClient> ApiClients { get; set; }
    
    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    base.OnModelCreating(modelBuilder);

    //    modelBuilder.Entity<User>()
    //            .HasOne(u => u.ApiClient)
    //            .WithMany()
    //            .OnDelete(DeleteBehavior.Restrict);
    //}
}