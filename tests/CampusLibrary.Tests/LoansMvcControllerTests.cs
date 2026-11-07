using CampusLibrary.Data;
using CampusLibrary.WebMvc.Controllers;
using CampusLibrary.WebMvc.Models.Loans;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CampusLibrary.Tests;

public class LoansMvcControllerTests
{
    private static LibraryDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }

    private static Member NewMember(string fullName, string email) => new() { FullName = fullName, Email = email };

    private static LoansController NewController(LibraryDbContext db)
    {
        var controller = new LoansController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };
        controller.TempData = new TempDataDictionary(controller.HttpContext,
            Mock.Of<ITempDataProvider>());
        return controller;
    }
    private static Book NewBook(string title, string author, string isbn) => new() { Title = title, Author = author, ISBN = isbn, AvailableCopies = 2 };

    private static async Task<(Member Member, Book Book, Loan OpenLoan)> SeedOpenLoanAsync(LibraryDbContext db)
    {
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Members.Add(member);
        db.Books.Add(book);
        await db.SaveChangesAsync();

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow };
        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        return (member, book, loan);
    }

    [Fact]
    public async Task Loans_Index_ReturnsOnlyOpenLoansWithBookMemberAndBorrowedAt()
    {
        await using var db = CreateDatabase();
        var (member, book, loan) = await SeedOpenLoanAsync(db);
        var controller = new LoansController(db);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<LoanIndexViewModel>(viewResult.Model);
        Assert.Single(model.OpenLoans);
        Assert.Equal(loan.Id, model.OpenLoans[0].LoanId);
        Assert.Equal(book.Title, model.OpenLoans[0].BookTitle);
        Assert.Equal(book.Author, model.OpenLoans[0].BookAuthor);
        Assert.Equal(member.FullName, model.OpenLoans[0].MemberName);
    }

    [Fact]
    public async Task Loans_Index_ExcludesAlreadyReturnedLoans()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var openBook = NewBook("Open Book", "Author", "1111111111111");
        var returnedBook = NewBook("Returned Book", "Author", "2222222222222");
        db.Members.Add(member);
        db.Books.Add(openBook);
        db.Books.Add(returnedBook);
        await db.SaveChangesAsync();

        db.Loans.Add(new Loan { BookId = openBook.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow });
        db.Loans.Add(new Loan { BookId = returnedBook.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow.AddDays(-2), ReturnedAt = DateTime.UtcNow.AddDays(-1) });
        await db.SaveChangesAsync();
        var controller = new LoansController(db);

        var model = Assert.IsType<LoanIndexViewModel>(Assert.IsType<ViewResult>(await controller.Index()).Model);
        Assert.Single(model.OpenLoans);
        Assert.Equal(openBook.Title, model.OpenLoans[0].BookTitle);
    }

    [Fact]
    public async Task Loans_Index_ReturnsEmptyWhenNoOpenLoans()
    {
        await using var db = CreateDatabase();
        var controller = new LoansController(db);

        var model = Assert.IsType<LoanIndexViewModel>(Assert.IsType<ViewResult>(await controller.Index()).Model);
        Assert.Empty(model.OpenLoans);
    }

    [Fact]
    public async Task Loans_Return_OpenLoan_ReturnsRedirectAndSetsReturnedAt()
    {
        await using var db = CreateDatabase();
        var (_, _, loan) = await SeedOpenLoanAsync(db);
        var controller = NewController(db);

        var result = await controller.Return(loan.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LoansController.Index), redirect.ActionName);
        Assert.NotNull(db.Loans.Single(l => l.Id == loan.Id).ReturnedAt);
        Assert.Equal("Loan marked as returned.", controller.TempData["Success"]);
        Assert.Null(controller.TempData["Error"]);
    }

    [Fact]
    public async Task Loans_Return_MissingLoan_ReturnsRedirectWithError()
    {
        await using var db = CreateDatabase();
        var controller = NewController(db);

        var result = await controller.Return(99);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LoansController.Index), redirect.ActionName);
        Assert.Equal("Loan not found.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task Loans_Return_AlreadyReturnedLoan_ReturnsRedirectWithError()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Members.Add(member);
        db.Books.Add(book);
        await db.SaveChangesAsync();

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow.AddDays(-2), ReturnedAt = DateTime.UtcNow.AddDays(-1) };
        db.Loans.Add(loan);
        await db.SaveChangesAsync();
        var controller = NewController(db);

        var result = await controller.Return(loan.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LoansController.Index), redirect.ActionName);
        Assert.Equal("This loan has already been returned.", controller.TempData["Error"]);
        Assert.Equal(loan.ReturnedAt.Value.Date, db.Loans.Single(l => l.Id == loan.Id).ReturnedAt.GetValueOrDefault().Date);
    }

    [Fact]
    public void Loans_Return_HasAntiforgeryAttribute()
    {
        var method = typeof(LoansController).GetMethod(nameof(LoansController.Return))!;
        Assert.Contains(method.GetCustomAttributes(inherit: true),
            a => a.GetType().Name == nameof(Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute));
    }
}
