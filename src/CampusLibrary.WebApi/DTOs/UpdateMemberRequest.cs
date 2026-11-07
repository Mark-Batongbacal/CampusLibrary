using System.ComponentModel.DataAnnotations;

namespace CampusLibrary.WebApi.DTOs;

public class UpdateMemberRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}