namespace CampusLibrary.WebMvc.Models.Loans;

public record OpenLoanSummary(int LoanId, string BookTitle, string BookAuthor, string MemberName, DateTime BorrowedAt);
