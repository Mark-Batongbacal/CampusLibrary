using System.ComponentModel.DataAnnotations;

namespace CampusLibrary.WebApi.DTOs;

public class ReturnBookRequest
{
    [Range(1, int.MaxValue)]
    public int LoanId { get; set; }
}