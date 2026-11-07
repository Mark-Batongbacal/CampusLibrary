using System.Collections.Generic;

namespace CampusLibrary.WebMvc.Models.Members;

public class MemberDetailsViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<LoanSummary> Loans { get; set; } = new();

    public record LoanSummary(int LoanId, string BookTitle, DateTime BorrowedAt, DateTime? ReturnedAt);
}
