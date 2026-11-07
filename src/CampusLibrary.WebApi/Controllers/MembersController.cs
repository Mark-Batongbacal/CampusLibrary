using CampusLibrary.Data;
using CampusLibrary.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.WebApi.Controllers;

[ApiController]
[Route("api/members")]
public class MembersController(LibraryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetAll()
    {
        var members = await db.Members.AsNoTracking()
            .Select(member => MemberDto.From(member))
            .ToListAsync();
        return Ok(members);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberDto>> GetById(int id)
    {
        var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return member is null ? NotFound() : Ok(MemberDto.From(member));
    }

    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create(CreateMemberRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check for duplicate email
        if (await db.Members.AnyAsync(m => m.Email == request.Email))
            return Conflict(new ProblemDetails { Title = "A member with this email already exists.", Status = StatusCodes.Status409Conflict });

        var member = new Member
        {
            FullName = request.FullName,
            Email = request.Email
        };
        db.Members.Add(member);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return Conflict(new ProblemDetails { Title = "A member with this email already exists.", Status = StatusCodes.Status409Conflict });
        }

        var response = MemberDto.From(member);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateMemberRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var member = await db.Members.FindAsync(id);
        if (member is null)
            return NotFound();

        member.FullName = request.FullName;
        member.Email = request.Email;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return Conflict(new ProblemDetails { Title = "A member with this email already exists.", Status = StatusCodes.Status409Conflict });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await db.Members.FindAsync(id);
        if (member is null)
            return NotFound();

        // Check for existing loan history - reject deletion with 409
        var hasLoans = await db.Loans.AnyAsync(l => l.MemberId == id);
        if (hasLoans)
            return Conflict(new ProblemDetails { Title = "Member has borrowing records and cannot be deleted.", Status = StatusCodes.Status409Conflict });

        db.Members.Remove(member);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return Conflict(new ProblemDetails { Title = "Cannot delete member due to foreign key constraint.", Status = StatusCodes.Status409Conflict });
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