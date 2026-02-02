using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BookTransaction> BookTransactions { get; set; }
        public object Transactions { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Book entity
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasIndex(b => b.ISBN).IsUnique();
                entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
                entity.Property(b => b.Author).IsRequired().HasMaxLength(100);
                entity.Property(b => b.ISBN).IsRequired().HasMaxLength(20);
                entity.Property(b => b.TotalCopies).HasDefaultValue(1);
                entity.Property(b => b.AvailableCopies).HasDefaultValue(1);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.IsActive).HasDefaultValue(true);
            });

            // Configure BookTransaction entity
            modelBuilder.Entity<BookTransaction>(entity =>
            {
                entity.HasOne(t => t.Book)
                    .WithMany()
                    .HasForeignKey(t => t.BookId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(t => t.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Issued");

                entity.Property(t => t.FineAmount)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);
            });
        }
    }
}
