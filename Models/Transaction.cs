#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment_Bank_Accounts.Models;

public class Transaction
{
    [Key]
    public int TransactionId { get; set; }

    [Display(Name = "Enter Amount: ")]
    [Required(ErrorMessage = "The amount is required")]
    [Range(0, double.MaxValue, ErrorMessage = "The amount must be greater than 0")]
    public double Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int UserId { get; set; }

    public User user { get; set; }

}