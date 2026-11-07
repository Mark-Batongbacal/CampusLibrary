using CampusLibrary.Data;
using CampusLibrary.WebMvc.Controllers;
using CampusLibrary.WebMvc.Models.Books;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.Tests;

public class BooksMvcControllerTests
{
    [Fact]
    public async Task Index_ReturnsAllBooks()
    {
        await using var db = CreateDatabase();
        db.Books.Add(NewBook("Alpha Book", "Author A", "1111111111111"));
        db.Books.Add(NewBook("Beta Book", "Author B", "2222222222222"));
        await db.SaveChangesAsync();
        var controller = new BooksController(db);

        var result = await controller.Index();

        var model = Assert.IsType<BookIndexViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(2, model.Books.Count);
        Assert.Equal("Alpha Book", model.Books[0].Title);
    }

    [Fact]
    public async Task Index_WhenEmpty_ReturnsEmptyList()
    {
        await using var db = CreateDatabase();
        var controller = new BooksController(db);

        var model = Assert.IsType<BookIndexViewModel>(Assert.IsType<ViewResult>(await controller.Index()).Model);
        Assert.Empty(model.Books);
    }

    [Fact]
    public async Task Edit_WhenPresent_ReturnsEditViewWithBook()
    {
        await using var db = CreateDatabase();
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var controller = new BooksController(db);

        var result = await controller.Edit(book.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Create", viewResult.ViewName);
        var model = Assert.IsType<BookFormViewModel>(viewResult.Model);
        Assert.Equal(book.Id, model.Id);
        Assert.Equal("Test Book", model.Title);
    }

    [Fact]
    public async Task Edit_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new BooksController(db);

        var result = await controller.Edit(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_WithValidBook_AddsBookAndRedirectsToIndex()
    {
        await using var db = CreateDatabase();
        var controller = new BooksController(db);

        var result = await controller.Create(new BookFormViewModel
        {
            Title = "New Book",
            Author = "New Author",
            ISBN = "3333333333333",
            AvailableCopies = 2
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BooksController.Index), redirect.ActionName);
        Assert.Single(db.Books);
        Assert.Equal("New Book", db.Books.Single().Title);
    }

    [Fact]
    public async Task Create_WithDuplicateISBN_AddsModelErrorAndReRedisplaysForm()
    {
        await using var db = CreateDatabase();
        db.Books.Add(NewBook("Existing", "Author", "dup-isbn-123"));
        await db.SaveChangesAsync();
        var controller = new BooksController(db);

        var result = await controller.Create(new BookFormViewModel
        {
            Title = "Another",
            Author = "Author",
            ISBN = "dup-isbn-123",
            AvailableCopies = 1
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Contains(nameof(BookFormViewModel.ISBN), viewResult.ViewData.ModelState.Keys);
        Assert.Single(db.Books);
    }

    [Fact]
    public async Task Edit_WithValidBook_UpdatesBookAndRedirectsToIndex()
    {
        await using var db = CreateDatabase();
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var controller = new BooksController(db);

        var result = await controller.Edit(book.Id, new BookFormViewModel
        {
            Id = book.Id,
            Title = "Updated Book",
            Author = "Updated Author",
            ISBN = "1234567890123",
            AvailableCopies = 5
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BooksController.Index), redirect.ActionName);
        Assert.Equal("Updated Book", db.Books.Single().Title);
        Assert.Equal(5, db.Books.Single().AvailableCopies);
    }

    [Fact]
    public async Task Edit_WithDuplicateISBN_AddsModelError()
    {
        await using var db = CreateDatabase();
        db.Books.Add(NewBook("Book One", "Author", "isbn-one"));
        db.Books.Add(NewBook("Book Two", "Author", "isbn-two"));
        await db.SaveChangesAsync();
        var target = db.Books.First(b => b.Title == "Book One");
        var controller = new BooksController(db);

        var result = await controller.Edit(target.Id, new BookFormViewModel
        {
            Id = target.Id,
            Title = "Book One",
            Author = "Author",
            ISBN = "isbn-two",
            AvailableCopies = 1
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Contains(nameof(BookFormViewModel.ISBN), viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Delete_WhenPresent_RemovesBookAndRedirectsToIndex()
    {
        await using var db = CreateDatabase();
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var controller = new BooksController(db);

        var result = await controller.Delete(book.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BooksController.Index), redirect.ActionName);
        Assert.Empty(db.Books);
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new BooksController(db);

        var result = await controller.Delete(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Create_HasAntiforgeryAttribute()
    {
        var postCreate = typeof(BooksController).GetMethods()
            .First(m => m.Name == nameof(BooksController.Create)
                && m.GetCustomAttributes(inherit: true).Any(a => a.GetType().Name == nameof(HttpPostAttribute)));
        Assert.Contains(postCreate.GetCustomAttributes(inherit: true),
            a => a.GetType().Name == nameof(Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute));
    }

    [Fact]
    public void EditPost_HasAntiforgeryAttribute()
    {
        var postEdit = typeof(BooksController).GetMethods()
            .First(m => m.Name == nameof(BooksController.Edit)
                && m.GetCustomAttributes(inherit: true).Any(a => a.GetType().Name == nameof(HttpPostAttribute)));
        Assert.Contains(postEdit.GetCustomAttributes(inherit: true),
            a => a.GetType().Name == nameof(Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute));
    }

    [Fact]
    public void Delete_HasAntiforgeryAttribute()
    {
        var method = typeof(BooksController).GetMethod(nameof(BooksController.Delete))!;
        Assert.Contains(method.GetCustomAttributes(inherit: true),
            a => a.GetType().Name == nameof(Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute));
    }

    private static Book NewBook(string title, string author, string isbn) => new()
    {
        Title = title,
        Author = author,
        ISBN = isbn,
        AvailableCopies = 2
    };

    private static LibraryDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }
}
