using CampusLibrary.Data;
using CampusLibrary.WebMvc.Models.Loans;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.WebMvc.Controllers;

public class LoansController(LibraryDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var openLoans = await db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.ReturnedAt == null)
            .OrderByDescending(l => l.BorrowedAt)
            .Select(l => new OpenLoanSummary(
                l.Id,
                l.Book!.Title,
                l.Book!.Author,
                l.Member!.FullName,
                l.BorrowedAt))
            .ToListAsync();

        return View(new LoanIndexViewModel(openLoans));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id)
    {
        // Check loan exists and is open before attempting update
        var loan = await db.Loans.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        if (loan is null)
        {
            TempData["Error"] = "Loan not found.";
            return RedirectToAction(nameof(Index));
        }

        if (loan.ReturnedAt is not null)
        {
            TempData["Error"] = "This loan has already been returned.";
            return RedirectToAction(nameof(Index));
        }

        // Try atomic update first (prevents race conditions on SQL Server)
        // Fall back to entity update for providers that don't support ExecuteUpdateAsync (e.g., InMemory in tests)
        var affected = 0;
        try
        {
            affected = await db.Loans
                .Where(l => l.Id == id && l.ReturnedAt == null)
                .ExecuteUpdateAsync(l => l.SetProperty(l => l.ReturnedAt, DateTime.UtcNow));
        }
        catch (InvalidOperationException) when (!db.Database.IsSqlServer())
        {
            // Provider doesn't support ExecuteUpdateAsync (e.g., InMemory) — fall back to entity update
            var trackedLoan = await db.Loans.FirstOrDefaultAsync(l => l.Id == id && l.ReturnedAt == null);
            if (trackedLoan is not null)
            {
                trackedLoan.ReturnedAt = DateTime.UtcNow;
                affected = await db.SaveChangesAsync();
            }
        }

        if (affected == 0)
        {
            TempData["Error"] = "This loan has already been returned.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Loan marked as returned.";
        return RedirectToAction(nameof(Index));
    }
}
