using CampusLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.WebMvc.Controllers;

public class HomeController(LibraryDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var bookCount = await db.Books.CountAsync();
        var memberCount = await db.Members.CountAsync();
        var openLoanCount = await db.Loans.CountAsync(l => l.ReturnedAt == null);

        ViewData["BookCount"] = bookCount;
        ViewData["MemberCount"] = memberCount;
        ViewData["OpenLoanCount"] = openLoanCount;

        return View();
    }
}
