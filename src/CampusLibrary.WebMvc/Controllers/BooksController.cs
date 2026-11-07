using CampusLibrary.Data;
using CampusLibrary.WebMvc.Models.Books;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.WebMvc.Controllers;

public class BooksController(LibraryDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var books = await db.Books
            .AsNoTracking()
            .OrderBy(b => b.Title)
            .Select(b => new BookSummary(b.Id, b.Title, b.Author, b.ISBN, b.AvailableCopies))
            .ToListAsync();

        return View(new BookIndexViewModel(books));
    }

    [HttpGet]
    public IActionResult Create() => View(new BookFormViewModel());

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var book = await db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (book is null)
            return NotFound();

        return View("Create", new BookFormViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            AvailableCopies = book.AvailableCopies
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await db.Books.AnyAsync(b => b.ISBN == model.ISBN))
        {
            ModelState.AddModelError(nameof(BookFormViewModel.ISBN),
                "A book with this ISBN already exists.");
            return View(model);
        }

        db.Books.Add(new Book
        {
            Title = model.Title,
            Author = model.Author,
            ISBN = model.ISBN,
            AvailableCopies = model.AvailableCopies
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(BookFormViewModel.ISBN),
                "A book with this ISBN already exists.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (id != model.Id)
            return NotFound();

        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book is null)
            return NotFound();

        if (await db.Books.AnyAsync(b => b.ISBN == model.ISBN && b.Id != model.Id))
        {
            ModelState.AddModelError(nameof(BookFormViewModel.ISBN),
                "A book with this ISBN already exists.");
            return View(model);
        }

        book.Title = model.Title;
        book.Author = model.Author;
        book.ISBN = model.ISBN;
        book.AvailableCopies = model.AvailableCopies;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(BookFormViewModel.ISBN),
                "A book with this ISBN already exists.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book is null)
            return NotFound();

        // A referenced book (open loan) must not be deleted; preserve existing borrowing records.
        if (await db.Loans.AnyAsync(l => l.BookId == id && l.ReturnedAt == null))
        {
            TempData["DeleteConflict"] =
                $"Cannot delete '{book.Title}' — it has an open loan. Return the loan first.";
            return RedirectToAction(nameof(Index));
        }

        db.Books.Remove(book);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            TempData["DeleteConflict"] =
                $"Cannot delete '{book.Title}' — it is referenced by borrowing records.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
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
