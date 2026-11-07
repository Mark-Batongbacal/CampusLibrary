using CampusLibrary.Data;
using CampusLibrary.WebMvc.Models.Members;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.WebMvc.Controllers;

public class MembersController(LibraryDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var members = await db.Members
            .AsNoTracking()
            .OrderBy(m => m.FullName)
            .Select(m => new MemberIndexViewModel.MemberSummary(m.Id, m.FullName, m.Email))
            .ToListAsync();

        return View(new MemberIndexViewModel(members));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var member = await db.Members
            .AsNoTracking()
            .Include(m => m.Loans)
            .ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member is null)
            return NotFound();

        return View(new MemberDetailsViewModel
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            Loans = member.Loans
                .OrderByDescending(l => l.BorrowedAt)
                .Select(l => new MemberDetailsViewModel.LoanSummary(
                    l.Id,
                    l.Book!.Title,
                    l.BorrowedAt,
                    l.ReturnedAt))
                .ToList()
        });
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMemberViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await db.Members.AnyAsync(m => m.Email == model.Email))
        {
            ModelState.AddModelError(nameof(CreateMemberViewModel.Email),
                "A member with this email already exists.");
            return View(model);
        }

        var member = new Member
        {
            FullName = model.FullName,
            Email = model.Email
        };
        db.Members.Add(member);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(CreateMemberViewModel.Email),
                "A member with this email already exists.");
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = member.Id });
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        FindSqlException(exception) is { Number: 2601 or 2627 };

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
