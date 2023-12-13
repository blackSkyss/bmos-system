using BMOS.DAL.DataSeedings;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DBContext
{
    public class BMOSDbContext : DbContext
    {
        public BMOSDbContext()
        {

        }

        public BMOSDbContext(DbContextOptions<BMOSDbContext> options) : base(options)
        {

        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<MealImage> MealImages { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductMeal> ProductMeals { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<OrderLog> OrderLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                IConfigurationRoot configuration = builder.Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("MyDbStore"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Role
            modelBuilder.Entity<Role>(role =>
            {
                role.Property(prop => prop.Name).IsUnicode(true).HasMaxLength(50);
            });
            #endregion

            #region Token
            modelBuilder.Entity<Token>(token =>
            {
                token.Property(prop => prop.JwtID).IsUnicode(false).HasMaxLength(int.MaxValue);
                token.Property(prop => prop.RefreshToken).IsUnicode(false).HasMaxLength(int.MaxValue);
                token.Property(prop => prop.CreatedDate).HasColumnType("datetime2");
                token.Property(prop => prop.ExpiredDate).HasColumnType("datetime2");

            });
            #endregion

            #region Account
            modelBuilder.Entity<Account>(account =>
            {
                account.Property(prop => prop.Email).IsUnicode(false).HasMaxLength(100);
                account.Property(prop => prop.PasswordHash).IsUnicode(false).HasMaxLength(int.MaxValue);
            });
            #endregion

            #region Staff
            modelBuilder.Entity<Staff>()
                        .HasOne(staff => staff.Account)
                        .WithOne()
                        .HasForeignKey<Staff>(staff => staff.AccountID);
            modelBuilder.Entity<Staff>(staff =>
            {
                staff.Property(prop => prop.FullName).IsUnicode(true).HasMaxLength(50);
                staff.Property(prop => prop.Address).IsUnicode(true).HasMaxLength(100);
                staff.Property(prop => prop.Phone).IsUnicode(false).HasMaxLength(10);
                staff.Property(prop => prop.Avatar).IsUnicode(false).HasMaxLength(int.MaxValue);
                staff.Property(prop => prop.BirthDate).HasColumnType("datetime2");
                staff.Property(prop => prop.IdentityNumber).IsUnicode(false).HasMaxLength(12);
                staff.Property(prop => prop.RegisteredDate).HasColumnType("datetime2");
                staff.Property(prop => prop.QuitDate).HasColumnType("datetime2").IsRequired(false);

            });
            #endregion

            #region Customer
            modelBuilder.Entity<Customer>()
                        .HasOne(customer => customer.Account)
                        .WithOne()
                        .HasForeignKey<Customer>(customer => customer.AccountID);
            modelBuilder.Entity<Customer>(customer =>
            {
                customer.Property(prop => prop.FullName).IsUnicode(true).HasMaxLength(80);
                customer.Property(prop => prop.Address).IsUnicode(true).HasMaxLength(120);
                customer.Property(prop => prop.Phone).IsUnicode(false).HasMaxLength(10);
                customer.Property(prop => prop.Avatar).IsUnicode(false).HasMaxLength(int.MaxValue);
                customer.Property(prop => prop.AvatarID).IsUnicode(false).HasMaxLength(int.MaxValue);
                customer.Property(prop => prop.BirthDate).HasColumnType("datetime2");
            });
            #endregion

            #region Meal
            modelBuilder.Entity<Meal>(meal =>
            {
                meal.Property(prop => prop.Title).IsUnicode(true).HasMaxLength(120);
                meal.Property(prop => prop.Description).IsUnicode(true).HasMaxLength(int.MaxValue);
                meal.Property(prop => prop.ModifiedDate).HasColumnType("datetime2");
            });
            #endregion

            #region MealImage
            modelBuilder.Entity<MealImage>(mealImage =>
            {
                mealImage.Property(prop => prop.ImageID).IsUnicode(false).HasMaxLength(int.MaxValue);
                mealImage.Property(prop => prop.Source).IsUnicode(false).HasMaxLength(int.MaxValue);
            });
            #endregion

            #region Order
            modelBuilder.Entity<Order>(order =>
            {
                order.Property(prop => prop.OrderedDate).HasColumnType("datetime2");
            });
            #endregion

            #region OrderLog
            modelBuilder.Entity<OrderLog>(orderLog =>
            {
                orderLog.Property(prop => prop.PaymentTime).HasColumnType("datetime2");
            });
            #endregion

            #region OrderDetail
            modelBuilder.Entity<OrderDetail>().HasKey("OrderID", "MealID");
            #endregion

            #region Product
            modelBuilder.Entity<Product>(product =>
            {
                product.Property(prop => prop.Name).IsUnicode(true).HasMaxLength(120);
                product.Property(prop => prop.Description).IsUnicode(true).HasMaxLength(int.MaxValue);
                product.Property(prop => prop.ImportedTime).HasColumnType("datetime2");
                product.Property(prop => prop.ExpiredDate).HasColumnType("datetime2");
                product.Property(prop => prop.ModifiedDate).HasColumnType("datetime2");

            });
            #endregion

            #region ProductMeal
            modelBuilder.Entity<ProductMeal>().HasKey("ProductID", "MealID");
            #endregion

            #region ProductImage
            modelBuilder.Entity<ProductImage>(productImage =>
            {
                productImage.Property(prop => prop.ImageID).IsUnicode(false).HasMaxLength(int.MaxValue);
                productImage.Property(prop => prop.Source).IsUnicode(false).HasMaxLength(int.MaxValue);
            });
            #endregion

            #region Wallet
            modelBuilder.Entity<Wallet>()
                        .HasOne(wallet => wallet.Account)
                        .WithOne()
                        .HasForeignKey<Wallet>(wallet => wallet.AccountID);
            #endregion

            #region WalletTransaction
            modelBuilder.Entity<WalletTransaction>(walletTransaction =>
            {
                walletTransaction.Property(prop => prop.RechargeID).IsUnicode(false).HasMaxLength(int.MaxValue);
                walletTransaction.Property(prop => prop.RechargeTime).HasColumnType("datetime2");
                walletTransaction.Property(prop => prop.RechargeStatus).HasColumnType("int");
                walletTransaction.Property(prop => prop.Content).IsUnicode(true).HasMaxLength(int.MaxValue);
                walletTransaction.Property(prop => prop.TransactionType).IsUnicode(false).HasMaxLength(50);
            });
            #endregion

            #region DataSeeding
            modelBuilder.RoleData();
            modelBuilder.MealData();
            modelBuilder.ProductData();
            modelBuilder.ProducMealtData();
            #endregion
        }
    }
}
