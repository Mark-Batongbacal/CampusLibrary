using System.Collections.Generic;

namespace CampusLibrary.WebMvc.Models.Members;

public class MemberIndexViewModel
{
    public List<MemberSummary> Members { get; }

    public MemberIndexViewModel(IEnumerable<MemberSummary> members)
    {
        Members = new List<MemberSummary>(members);
    }

    public record MemberSummary(int Id, string FullName, string Email);
}
