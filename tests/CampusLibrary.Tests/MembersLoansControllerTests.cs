using CampusLibrary.Data;
using CampusLibrary.WebApi.Controllers;
using CampusLibrary.WebApi.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.Tests;

public class MembersLoansControllerTests
{
    private static LibraryDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }

    private static Member NewMember(string fullName, string email) => new() { FullName = fullName, Email = email };
    private static Book NewBook(string title, string author, string isbn) => new() { Title = title, Author = author, ISBN = isbn, AvailableCopies = 2 };

    private static CreateMemberRequest NewCreateMemberRequest(string fullName = "Test Member", string email = "test@university.edu") => new() { FullName = fullName, Email = email };
    private static UpdateMemberRequest NewUpdateMemberRequest(string fullName = "Updated Member", string email = "updated@university.edu") => new() { FullName = fullName, Email = email };
    private static BorrowBookRequest NewBorrowRequest(int memberId = 1, int bookId = 1) => new() { MemberId = memberId, BookId = bookId };

    /* ---------- Members ---------- */

    [Fact]
    public async Task Members_GetAll_ReturnsMemberDtos()
    {
        await using var db = CreateDatabase();
        db.Members.Add(NewMember("Alice Johnson", "alice.johnson@university.edu"));
        await db.SaveChangesAsync();
        var controller = new MembersController(db);

        var result = await controller.GetAll();

        var members = Assert.IsType<OkObjectResult>(result.Result).Value;
        var member = Assert.IsAssignableFrom<IEnumerable<MemberDto>>(members).Single();
        Assert.Equal("Alice Johnson", member.FullName);
        Assert.IsNotType<Member>(members);
    }

    [Fact]
    public async Task Members_GetById_WhenPresent_ReturnsMemberDto()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        db.Members.Add(member);
        await db.SaveChangesAsync();
        var controller = new MembersController(db);

        var result = await controller.GetById(member.Id);

        var dto = Assert.IsType<MemberDto>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("Alice Johnson", dto.FullName);
        Assert.Equal("alice.johnson@university.edu", dto.Email);
    }

    [Fact]
    public async Task Members_GetById_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);

        var result = await controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Members_Create_WithValidMember_ReturnsCreatedDto()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);

        var result = await controller.Create(NewCreateMemberRequest());

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.IsType<MemberDto>(created.Value);
        Assert.Single(db.Members);
    }

    [Fact]
    public async Task Members_Create_WithDuplicateEmail_ReturnsConflict()
    {
        await using var db = CreateDatabase();
        db.Members.Add(NewMember("Existing", "dup@university.edu"));
        await db.SaveChangesAsync();
        var controller = new MembersController(db);

        var result = await controller.Create(NewCreateMemberRequest(fullName: "Another", email: "dup@university.edu"));

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
        Assert.Equal(1, db.Members.Count());
    }

    [Fact]
    public async Task Members_Create_WithInvalidModelState_ReturnsBadRequest()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);
        controller.ModelState.AddModelError(nameof(CreateMemberRequest.Email), "Email is required");

        var result = await controller.Create(new CreateMemberRequest { FullName = "Test", Email = string.Empty });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(db.Members);
    }

    [Fact]
    public async Task Members_Update_WithValidMember_ChangesMemberAndReturnsNoContent()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        db.Members.Add(member);
        await db.SaveChangesAsync();
        var controller = new MembersController(db);

        var result = await controller.Update(member.Id, NewUpdateMemberRequest());

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Updated Member", member.FullName);
        Assert.Equal("updated@university.edu", member.Email);
    }

    [Fact]
    public async Task Members_Update_WithInvalidModelState_ReturnsBadRequestWithoutChangingMember()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        db.Members.Add(member);
        await db.SaveChangesAsync();
        var controller = new MembersController(db);
        controller.ModelState.AddModelError(nameof(UpdateMemberRequest.Email), "Email is required");

        var result = await controller.Update(member.Id, new UpdateMemberRequest { FullName = "Changed", Email = string.Empty });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Alice Johnson", member.FullName);
        Assert.Equal("alice.johnson@university.edu", member.Email);
    }

    [Fact]
    public async Task Members_Update_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);

        var result = await controller.Update(99, NewUpdateMemberRequest());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Members_Delete_WhenPresent_RemovesMemberAndReturnsNoContent()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        db.Members.Add(member);
        await db.SaveChangesAsync();
        var controller = new MembersController(db);

        var result = await controller.Delete(member.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(db.Members);
    }

    [Fact]
    public async Task Members_Delete_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);

        var result = await controller.Delete(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Members_Delete_WithLoanHistory_ReturnsConflict()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Members.Add(member);
        db.Books.Add(book);
        await db.SaveChangesAsync();

        db.Loans.Add(new Loan { BookId = book.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var controller = new MembersController(db);

        var result = await controller.Delete(member.Id);

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(db.Members);
        Assert.Single(db.Loans);
    }

    /* ---------- Loans ---------- */

    [Fact]
    public async Task Loans_GetAll_ReturnsLoanDtos()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Members.Add(member);
        db.Books.Add(book);
        await db.SaveChangesAsync();

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow };
        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        var controller = new LoansController(db);

        var result = await controller.GetAll();

        var loans = Assert.IsType<OkObjectResult>(result.Result).Value;
        var loanDto = Assert.IsAssignableFrom<IEnumerable<LoanDto>>(loans).Single();
        Assert.Equal(book.Id, loanDto.BookId);
        Assert.Equal(book.Title, loanDto.BookTitle);
        Assert.Equal(member.Id, loanDto.MemberId);
        Assert.Equal(member.FullName, loanDto.MemberName);
    }

    [Fact]
    public async Task Loans_GetById_WhenPresent_ReturnsLoanDto()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Members.Add(member);
        db.Books.Add(book);
        await db.SaveChangesAsync();

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow };
        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        var controller = new LoansController(db);

        var result = await controller.GetById(loan.Id);

        var dto = Assert.IsType<LoanDto>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(book.Title, dto.BookTitle);
        Assert.Equal(member.FullName, dto.MemberName);
    }

    [Fact]
    public async Task Loans_GetById_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new LoansController(db);

        var result = await controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Loans_Borrow_WithValidBook_ReturnsCreatedDto()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Members.Add(member);
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var controller = new LoansController(db);

        var result = await controller.Borrow(NewBorrowRequest(member.Id, book.Id));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.IsType<LoanDto>(created.Value);
        Assert.Single(db.Loans);
        Assert.Null(loanInDatabase(db, book.Id).ReturnedAt);
    }

    [Fact]
    public async Task Loans_Borrow_WithUnknownMember_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Books.Add(book);
        await db.SaveChangesAsync();
        var controller = new LoansController(db);

        var result = await controller.Borrow(NewBorrowRequest(memberId: 99, bookId: book.Id));

        Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Empty(db.Loans);
    }

    [Fact]
    public async Task Loans_Borrow_WithUnknownBook_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        db.Members.Add(member);
        await db.SaveChangesAsync();
        var controller = new LoansController(db);

        var result = await controller.Borrow(NewBorrowRequest(memberId: member.Id, bookId: 99));

        Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Empty(db.Loans);
    }

    [Fact]
    public async Task Loans_Borrow_BookWithOpenLoan_ReturnsConflict()
    {
        await using var db = CreateDatabase();
        var member1 = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var member2 = NewMember("Bob Smith", "bob.smith@university.edu");
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Members.Add(member1);
        db.Members.Add(member2);
        db.Books.Add(book);
        await db.SaveChangesAsync();

        var existingLoan = new Loan { BookId = book.Id, MemberId = member1.Id, BorrowedAt = DateTime.UtcNow };
        db.Loans.Add(existingLoan);
        await db.SaveChangesAsync();

        var controller = new LoansController(db);

        var result = await controller.Borrow(NewBorrowRequest(member2.Id, book.Id));

        Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Single(db.Loans);
    }

    [Fact]
    public async Task Loans_Borrow_WithInvalidModelState_ReturnsBadRequest()
    {
        await using var db = CreateDatabase();
        var controller = new LoansController(db);
        controller.ModelState.AddModelError(nameof(BorrowBookRequest.BookId), "Book is required");

        var result = await controller.Borrow(new BorrowBookRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(db.Loans);
    }

    [Fact]
    public async Task Loans_Return_OpenLoan_ReturnsNoContentAndSetsReturnedAt()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Members.Add(member);
        db.Books.Add(book);
        await db.SaveChangesAsync();

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow };
        db.Loans.Add(loan);
        await db.SaveChangesAsync();
        var controller = new LoansController(db);

        var result = await controller.Return(loan.Id, new ReturnBookRequest { LoanId = loan.Id });

        Assert.IsType<NoContentResult>(result);
        var returnedLoan = loanInDatabase(db, loan.Id);
        Assert.NotNull(returnedLoan.ReturnedAt);
    }

    [Fact]
    public async Task Loans_Return_MissingLoan_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new LoansController(db);

        var result = await controller.Return(99, new ReturnBookRequest { LoanId = 99 });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Loans_Return_AlreadyReturnedLoan_ReturnsConflict()
    {
        await using var db = CreateDatabase();
        var member = NewMember("Alice Johnson", "alice.johnson@university.edu");
        var book = NewBook("Test Book", "Author", "1234567890123");
        db.Members.Add(member);
        db.Books.Add(book);
        await db.SaveChangesAsync();

        var loan = new Loan { BookId = book.Id, MemberId = member.Id, BorrowedAt = DateTime.UtcNow, ReturnedAt = DateTime.UtcNow.AddDays(-1) };
        db.Loans.Add(loan);
        await db.SaveChangesAsync();
        var controller = new LoansController(db);

        var result = await controller.Return(loan.Id, new ReturnBookRequest { LoanId = loan.Id });

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(loan.ReturnedAt.Value.Date, loanInDatabase(db, loan.Id).ReturnedAt.Value.Date);
    }

    [Fact]
    public async Task Loans_Borrow_WithZeroInventory_ReturnsConflict()
    {
        await using var db = CreateDatabase();
        var book = NewBook("Unavailable", "Author", "1234567890123");
        book.AvailableCopies = 0;
        var member = NewMember("Reader", "reader@example.test");
        db.AddRange(book, member);
        await db.SaveChangesAsync();
        var result = await new LoansController(db).Borrow(NewBorrowRequest(member.Id, book.Id));
        Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Empty(db.Loans);
    }

    [Fact]
    public async Task Loans_Return_MismatchedRoute_ReturnsBadRequest()
    {
        await using var db = CreateDatabase();
        var result = await new LoansController(db).Return(1, new ReturnBookRequest { LoanId = 2 });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Loans_Reborrow_PreservesReturnedHistory()
    {
        await using var db = CreateDatabase();
        var book = NewBook("Book", "Author", "1234567890123");
        var member = NewMember("Reader", "reader@example.test");
        db.AddRange(book, member);
        await db.SaveChangesAsync();
        var controller = new LoansController(db);
        var first = Assert.IsType<LoanDto>(Assert.IsType<CreatedAtActionResult>(
            (await controller.Borrow(NewBorrowRequest(member.Id, book.Id))).Result).Value);
        await controller.Return(first.Id, new ReturnBookRequest { LoanId = first.Id });
        var second = await controller.Borrow(NewBorrowRequest(member.Id, book.Id));
        Assert.IsType<CreatedAtActionResult>(second.Result);
        Assert.Equal(2, await db.Loans.CountAsync());
        Assert.Single(db.Loans.Where(l => l.ReturnedAt != null));
        Assert.Equal(2, book.Loans.Count);
    }

    private static Loan loanInDatabase(LibraryDbContext db, int loanId) =>
        db.Loans.Single(l => l.Id == loanId);
}