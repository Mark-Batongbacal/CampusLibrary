using System.Collections.Generic;

namespace CampusLibrary.WebMvc.Models.Loans;

public class LoanIndexViewModel
{
    public List<OpenLoanSummary> OpenLoans { get; }

    public LoanIndexViewModel(IEnumerable<OpenLoanSummary> openLoans)
    {
        OpenLoans = new List<OpenLoanSummary>(openLoans);
    }
}
