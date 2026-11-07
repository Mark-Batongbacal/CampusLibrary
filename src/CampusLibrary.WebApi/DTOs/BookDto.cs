using CampusLibrary.Data;

namespace CampusLibrary.WebApi.DTOs;

public record BookDto(int Id, string Title, string Author, string ISBN, int AvailableCopies, bool IsAvailable)
{
    public static BookDto From(Book book, bool hasOpenLoan = false) => new(book.Id, book.Title, book.Author, book.ISBN, book.AvailableCopies, book.AvailableCopies > 0 && !hasOpenLoan);
}