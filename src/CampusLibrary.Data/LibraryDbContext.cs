using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Book configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ISBN).IsRequired().HasMaxLength(13);
            entity.HasIndex(e => e.ISBN).IsUnique();
            entity.Property(e => e.AvailableCopies).IsRequired();

        });

        // Member configuration
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasMany(e => e.Loans)
                .WithOne(l => l.Member)
                .HasForeignKey(l => l.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Loan configuration
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BorrowedAt).IsRequired();
            entity.Property(e => e.ReturnedAt).IsRequired(false);

            entity.HasOne(e => e.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Member)
                .WithMany(m => m.Loans)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Filtered unique index to enforce at most one open loan per book
            entity.HasIndex(e => e.BookId)
                .IsUnique()
                .HasFilter("[ReturnedAt] IS NULL");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Five example books
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "The Pragmatic Programmer", Author = "Andrew Hunt, David Thomas", ISBN = "9780135957059", AvailableCopies = 3 },
            new Book { Id = 2, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780132350884", AvailableCopies = 2 },
            new Book { Id = 3, Title = "Design Patterns", Author = "Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides", ISBN = "9780201633610", AvailableCopies = 1 },
            new Book { Id = 4, Title = "Structure and Interpretation of Computer Programs", Author = "Harold Abelson, Gerald Jay Sussman", ISBN = "9780262510875", AvailableCopies = 2 },
            new Book { Id = 5, Title = "Introduction to Algorithms", Author = "Thomas H. Cormen, Charles E. Leiserson, Ronald L. Rivest, Clifford Stein", ISBN = "9780262033848", AvailableCopies = 4 }
        );

        // Three example members
        modelBuilder.Entity<Member>().HasData(
            new Member { Id = 1, FullName = "Alice Johnson", Email = "alice.johnson@university.edu" },
            new Member { Id = 2, FullName = "Bob Smith", Email = "bob.smith@university.edu" },
            new Member { Id = 3, FullName = "Carol Williams", Email = "carol.williams@university.edu" }
        );
    }
}