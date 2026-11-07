using System.ComponentModel.DataAnnotations;

namespace CampusLibrary.WebApi.DTOs;

public class BorrowBookRequest
{
    [Required]
    public int MemberId { get; set; }

    [Required]
    public int BookId { get; set; }
}