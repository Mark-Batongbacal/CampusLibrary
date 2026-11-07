using System.Collections.Generic;

namespace CampusLibrary.WebMvc.Models.Books;

public class BookIndexViewModel
{
    public List<BookSummary> Books { get; }

    public string? ErrorMessage { get; set; }

    public BookIndexViewModel(IEnumerable<BookSummary> books)
    {
        Books = new List<BookSummary>(books);
    }
}
