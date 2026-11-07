using System.ComponentModel.DataAnnotations;

namespace CampusLibrary.WebApi.DTOs;

public class ReturnBookRequest
{
    [Required]
    public int LoanId { get; set; }
}