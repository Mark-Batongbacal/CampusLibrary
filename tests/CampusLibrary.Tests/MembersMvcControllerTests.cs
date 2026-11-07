using CampusLibrary.Data;
using CampusLibrary.WebMvc.Controllers;
using CampusLibrary.WebMvc.Models.Members;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.Tests;

public class MembersMvcControllerTests
{
    [Fact]
    public async Task Index_ReturnsAllMembers()
    {
        await using var db = CreateDatabase();
        db.Members.Add(new Member { FullName = "Alice Johnson", Email = "alice@university.edu" });
        db.Members.Add(new Member { FullName = "Bob Smith", Email = "bob@university.edu" });
        await db.SaveChangesAsync();
        var controller = new MembersController(db);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MemberIndexViewModel>(viewResult.Model);
        Assert.Equal(2, model.Members.Count);
        Assert.Equal("Alice Johnson", model.Members[0].FullName);
    }

    [Fact]
    public async Task Index_WhenEmpty_ReturnsEmptyList()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);

        var result = await controller.Index();

        var model = Assert.IsType<MemberIndexViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Members);
    }

    [Fact]
    public async Task Details_WhenPresent_ReturnsMemberDetails()
    {
        await using var db = CreateDatabase();
        var member = new Member { FullName = "Alice Johnson", Email = "alice@university.edu" };
        db.Members.Add(member);
        await db.SaveChangesAsync();
        var controller = new MembersController(db);

        var result = await controller.Details(member.Id);

        var model = Assert.IsType<MemberDetailsViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(member.Id, model.Id);
        Assert.Equal("Alice Johnson", model.FullName);
    }

    [Fact]
    public async Task Details_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);

        var result = await controller.Details(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_GET_ReturnsView()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);

        var result = controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_WithValidMember_AddsMemberAndRedirectsToDetails()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);

        var result = await controller.Create(new CreateMemberViewModel
        {
            FullName = "New Member",
            Email = "new@university.edu"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(MembersController.Details), redirect.ActionName);
        Assert.Single(db.Members);
        Assert.Equal("new@university.edu", db.Members.Single().Email);
    }

    [Fact]
    public async Task Create_WithDuplicateEmail_AddsModelErrorAndReRedisplaysForm()
    {
        await using var db = CreateDatabase();
        db.Members.Add(new Member { FullName = "Existing", Email = "dup@university.edu" });
        await db.SaveChangesAsync();
        var controller = new MembersController(db);

        var result = await controller.Create(new CreateMemberViewModel
        {
            FullName = "Another",
            Email = "dup@university.edu"
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Contains(nameof(CreateMemberViewModel.Email), viewResult.ViewData.ModelState.Keys);
        Assert.Single(db.Members);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ReturnsFormWithoutCreatingMember()
    {
        await using var db = CreateDatabase();
        var controller = new MembersController(db);
        controller.ModelState.AddModelError(nameof(CreateMemberViewModel.Email), "Email is required");

        var result = await controller.Create(new CreateMemberViewModel { FullName = "Test", Email = string.Empty });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Empty(db.Members);
    }

    private static LibraryDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }
}
