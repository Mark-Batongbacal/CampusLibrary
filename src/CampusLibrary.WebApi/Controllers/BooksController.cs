using CampusLibrary.Data;
using CampusLibrary.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.WebApi.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController(LibraryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
    {
        var books = await db.Books.AsNoTracking()
            .Select(book => new BookDto(book.Id, book.Title, book.Author, book.ISBN, book.AvailableCopies,
                book.AvailableCopies > 0 && !db.Loans.Any(loan => loan.BookId == book.Id && loan.ReturnedAt == null)))
            .ToListAsync();
        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await db.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == id);
        if (book is null)
            return NotFound();

        var hasOpenLoan = await db.Loans.AnyAsync(loan => loan.BookId == id && loan.ReturnedAt == null);
        return Ok(BookDto.From(book, hasOpenLoan));
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(CreateBookRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            ISBN = request.ISBN,
            AvailableCopies = request.AvailableCopies
        };
        db.Books.Add(book);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return Conflict(new ProblemDetails { Title = "A book with this ISBN already exists.", Status = StatusCodes.Status409Conflict });
        }

        var response = BookDto.From(book);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateBookRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var book = await db.Books.FindAsync(id);
        if (book is null)
            return NotFound();

        book.Title = request.Title;
        book.Author = request.Author;
        book.ISBN = request.ISBN;
        book.AvailableCopies = request.AvailableCopies;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return Conflict(new ProblemDetails { Title = "A book with this ISBN already exists.", Status = StatusCodes.Status409Conflict });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await db.Books.FindAsync(id);
        if (book is null)
            return NotFound();

        db.Books.Remove(book);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return Conflict(new ProblemDetails { Title = "The book has borrowing records and cannot be deleted.", Status = StatusCodes.Status409Conflict });
        }

        return NoContent();
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        FindSqlException(exception) is { Number: 2601 or 2627 };

    private static bool IsForeignKeyViolation(DbUpdateException exception) =>
        FindSqlException(exception) is { Number: 547 };

    private static SqlException? FindSqlException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException!)
        {
            if (current is SqlException sqlException)
                return sqlException;
        }

        return null;
    }
}
