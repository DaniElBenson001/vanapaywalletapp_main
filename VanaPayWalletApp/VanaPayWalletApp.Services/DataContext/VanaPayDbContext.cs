using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using VanaPayWalletApp.Models;
using VanaPayWalletApp.Models.Entities;

namespace VanaPayWalletApp.DataContext
{
    public class VanapayDbContext : DbContext
    {

        public VanapayDbContext(DbContextOptions<VanapayDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionDataEntity>(entity =>
            {
                entity.HasOne(d => d.SenderUser)
                .WithMany(e => e.SenderTransaction)
                .HasForeignKey(f => f.SenderUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ReceiverUser)
                .WithMany(e => e.ReceiverTransaction)
                .HasForeignKey(f => f.ReceiverUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=VanaPayMainDB;Trusted_Connection=True;");
        //}

        public DbSet<UserDataEntity> Users { get; set; }
        public DbSet<TransactionDataEntity> Transactions { get; set; }
        public DbSet<AccountDataEntity> Accounts { get; set; }

    }
}
