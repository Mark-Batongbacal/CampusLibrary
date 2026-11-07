using CampusLibrary.Data;

namespace CampusLibrary.WebApi.DTOs;

public record LoanDto
(
    int Id,
    int BookId,
    string BookTitle,
    int MemberId,
    string MemberName,
    DateTime BorrowedAt,
    DateTime? ReturnedAt
)
{
    public static LoanDto From(Loan loan, BookSummaryDto book, MemberSummaryDto member)
        => new(
            loan.Id,
            loan.BookId,
            book.Title,
            loan.MemberId,
            member.FullName,
            loan.BorrowedAt,
            loan.ReturnedAt);
}

public record BookSummaryDto(int Id, string Title, string Author);
public record MemberSummaryDto(int Id, string FullName);