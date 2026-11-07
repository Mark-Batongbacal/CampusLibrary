using System.ComponentModel.DataAnnotations;

namespace CampusLibrary.WebApi.DTOs;

public class UpdateBookRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string Author { get; set; } = string.Empty;

    [Required, StringLength(13, MinimumLength = 1)]
    public string ISBN { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int AvailableCopies { get; set; }
}