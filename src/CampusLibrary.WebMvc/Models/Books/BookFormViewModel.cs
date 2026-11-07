using System.ComponentModel.DataAnnotations;

namespace CampusLibrary.WebMvc.Models.Books;

public class BookFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200, MinimumLength = 1)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    [Display(Name = "Author")]
    public string Author { get; set; } = string.Empty;

    [Required, StringLength(13, MinimumLength = 1)]
    [Display(Name = "ISBN")]
    public string ISBN { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    [Display(Name = "Available copies")]
    public int AvailableCopies { get; set; }
}
