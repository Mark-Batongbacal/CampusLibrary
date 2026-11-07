using CampusLibrary.Data;
using CampusLibrary.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.WebApi.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController(LibraryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetAll()
    {
        var loans = await db.Loans.AsNoTracking()
            .Select(loan => LoanDto.From(loan,
                new BookSummaryDto(loan.Book!.Id, loan.Book.Title, loan.Book.Author),
                new MemberSummaryDto(loan.Member!.Id, loan.Member.FullName)))
            .ToListAsync();
        return Ok(loans);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanDto>> GetById(int id)
    {
        var loan = await db.Loans.AsNoTracking()
            .Where(l => l.Id == id)
            .Select(loan => LoanDto.From(loan,
                new BookSummaryDto(loan.Book!.Id, loan.Book.Title, loan.Book.Author),
                new MemberSummaryDto(loan.Member!.Id, loan.Member.FullName)))
            .FirstOrDefaultAsync();

        return loan is null ? NotFound() : Ok(loan);
    }

    [HttpPost]
    public async Task<ActionResult<LoanDto>> Borrow(BorrowBookRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check that the member exists
        var member = await db.Members.FindAsync(request.MemberId);
        if (member is null)
            return NotFound(new ProblemDetails { Title = "Member not found.", Status = StatusCodes.Status404NotFound });

        // Check that the book exists
        var book = await db.Books.FindAsync(request.BookId);
        if (book is null)
            return NotFound(new ProblemDetails { Title = "Book not found.", Status = StatusCodes.Status404NotFound });

        if (book.AvailableCopies <= 0)
            return Conflict(new ProblemDetails { Title = "Book is unavailable.", Status = StatusCodes.Status409Conflict });

        // Check that no open loan exists for the book
        var hasOpenLoan = await db.Loans.AnyAsync(l => l.BookId == request.BookId && l.ReturnedAt == null);
        if (hasOpenLoan)
            return Conflict(new ProblemDetails { Title = "Book already has an open loan.", Status = StatusCodes.Status409Conflict });

        var loan = new Loan
        {
            BookId = request.BookId,
            MemberId = request.MemberId,
            BorrowedAt = DateTime.UtcNow
        };
        db.Loans.Add(loan);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return Conflict(new ProblemDetails { Title = "Book already has an open loan.", Status = StatusCodes.Status409Conflict });
        }

        var response = LoanDto.From(loan,
            new BookSummaryDto(book.Id, book.Title, book.Author),
            new MemberSummaryDto(member.Id, member.FullName));

        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, response);
    }

    [HttpPost("{id:int}/return")]
    public async Task<IActionResult> Return(int id, ReturnBookRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.LoanId != id)
            return BadRequest(new ProblemDetails { Title = "Loan ID does not match the route.", Status = StatusCodes.Status400BadRequest });

        var loan = await db.Loans.FindAsync(id);
        if (loan is null)
            return NotFound(new ProblemDetails { Title = "Loan not found.", Status = StatusCodes.Status404NotFound });

        // Confirm it has not already been returned
        if (loan.ReturnedAt is not null)
            return Conflict(new ProblemDetails { Title = "Loan has already been returned.", Status = StatusCodes.Status409Conflict });

        try
        {
            if (db.Database.IsRelational())
            {
                var affected = await db.Loans.Where(l => l.Id == id && l.ReturnedAt == null)
                    .ExecuteUpdateAsync(update => update.SetProperty(l => l.ReturnedAt, DateTime.UtcNow));
                if (affected == 0)
                    return Conflict(new ProblemDetails { Title = "Loan has already been returned.", Status = StatusCodes.Status409Conflict });
            }
            else
            {
                loan.ReturnedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return Conflict(new ProblemDetails { Title = "Cannot return loan due to foreign key constraint.", Status = StatusCodes.Status409Conflict });
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