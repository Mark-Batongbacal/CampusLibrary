using CampusLibrary.Data;
using CampusLibrary.WebApi.Controllers;
using CampusLibrary.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.Tests;

public class BooksControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsBookDtos()
    {
        await using var db = CreateDatabase();
        db.Books.Add(NewBook());
        await db.SaveChangesAsync();
        var controller = new BooksController(db);

        var result = await controller.GetAll();

        var books = Assert.IsType<OkObjectResult>(result.Result).Value;
        var book = Assert.IsAssignableFrom<IEnumerable<BookDto>>(books).Single();
        Assert.Equal("Test Book", book.Title);
        Assert.IsNotType<Book>(books);
    }

    [Fact]
    public async Task GetById_WhenPresent_ReturnsBookDto()
    {
        await using var db = CreateDatabase();
        var book = NewBook();
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var controller = new BooksController(db);

        var result = await controller.GetById(book.Id);

        Assert.Equal("Test Book", Assert.IsType<BookDto>(Assert.IsType<OkObjectResult>(result.Result).Value).Title);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new BooksController(db);

        var result = await controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithValidBook_ReturnsCreatedDto()
    {
        await using var db = CreateDatabase();
        var controller = new BooksController(db);

        var result = await controller.Create(NewCreateRequest());

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.IsType<BookDto>(created.Value);
        Assert.Single(db.Books);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ReturnsBadRequest()
    {
        await using var db = CreateDatabase();
        var controller = new BooksController(db);
        controller.ModelState.AddModelError(nameof(CreateBookRequest.Title), "Title is required");

        var result = await controller.Create(new CreateBookRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(db.Books);
    }

    [Fact]
    public async Task Update_WithValidBook_ChangesBookAndReturnsNoContent()
    {
        await using var db = CreateDatabase();
        var book = NewBook();
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var controller = new BooksController(db);

        var result = await controller.Update(book.Id, new UpdateBookRequest { Title = "Updated", Author = "New Author", ISBN = "1234567890124", AvailableCopies = 4 });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Updated", book.Title);
        Assert.Equal(4, book.AvailableCopies);
    }

    [Fact]
    public async Task Update_WithInvalidModelState_ReturnsBadRequestWithoutChangingBook()
    {
        await using var db = CreateDatabase();
        var book = NewBook();
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var controller = new BooksController(db);
        controller.ModelState.AddModelError(nameof(UpdateBookRequest.Title), "Title is required");

        var result = await controller.Update(book.Id, new UpdateBookRequest { Title = "Changed", Author = "Author", ISBN = book.ISBN, AvailableCopies = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Test Book", book.Title);
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new BooksController(db);

        var result = await controller.Update(99, NewUpdateRequest());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_WhenPresent_RemovesBookAndReturnsNoContent()
    {
        await using var db = CreateDatabase();
        var book = NewBook();
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var controller = new BooksController(db);

        var result = await controller.Delete(book.Id);

        Assert.IsType<NoContentResult>(result);
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
    public async Task Availability_TracksOpenLoanAndReturn()
    {
        await using var db = CreateDatabase();
        var book = NewBook();
        var member = new Member { FullName = "Reader", Email = "reader@example.test" };
        db.AddRange(book, member);
        await db.SaveChangesAsync();
        var controller = new BooksController(db);
        var available = await controller.GetById(book.Id);
        Assert.True(Assert.IsType<BookDto>(Assert.IsType<OkObjectResult>(available.Result).Value).IsAvailable);

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow };
        db.Loans.Add(loan);
        await db.SaveChangesAsync();
        var detail = await controller.GetById(book.Id);
        Assert.False(Assert.IsType<BookDto>(Assert.IsType<OkObjectResult>(detail.Result).Value).IsAvailable);
        var list = await controller.GetAll();
        Assert.False(Assert.IsAssignableFrom<IEnumerable<BookDto>>(Assert.IsType<OkObjectResult>(list.Result).Value).Single().IsAvailable);

        loan.ReturnedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var returned = await controller.GetById(book.Id);
        Assert.True(Assert.IsType<BookDto>(Assert.IsType<OkObjectResult>(returned.Result).Value).IsAvailable);
    }

    [Fact]
    public async Task Availability_WhenInventoryIsZero_IsFalse()
    {
        await using var db = CreateDatabase();
        var book = NewBook();
        book.AvailableCopies = 0;
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var result = await new BooksController(db).GetById(book.Id);
        Assert.False(Assert.IsType<BookDto>(Assert.IsType<OkObjectResult>(result.Result).Value).IsAvailable);
    }

    private static Book NewBook() => new() { Title = "Test Book", Author = "Author", ISBN = "1234567890123", AvailableCopies = 2 };
    private static CreateBookRequest NewCreateRequest() => new() { Title = "Test Book", Author = "Author", ISBN = "1234567890123", AvailableCopies = 2 };
    private static UpdateBookRequest NewUpdateRequest() => new() { Title = "Test Book", Author = "Author", ISBN = "1234567890123", AvailableCopies = 2 };

    private static LibraryDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }
}
