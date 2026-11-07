using System.ComponentModel.DataAnnotations;

namespace CampusLibrary.WebApi.DTOs;

public class BorrowBookRequest
{
    [Range(1, int.MaxValue)]
    public int MemberId { get; set; }

    [Range(1, int.MaxValue)]
    public int BookId { get; set; }
}