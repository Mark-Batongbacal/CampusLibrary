namespace CampusLibrary.Data;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int AvailableCopies { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}