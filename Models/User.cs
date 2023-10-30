#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Assignment_Bank_Accounts.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MinLength(2)]
    public string FirstName { get; set; }

    [Required]
    [MinLength(2)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [UniqueEmail]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<Transaction> Transactions { get; set; } = new List<Transaction>();

    [NotMapped]
    [Compare("Password", ErrorMessage = "Passwords are not the same")]
    [Required(ErrorMessage = "Confirmation is required")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [Display(Name = "Re-enter Password")]
    public string PasswordConfirm { get; set; }
}

public class UniqueEmailAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Aunque hemos requerido como validación, a veces lo hacemos aquí de todos modos
        // En cuyo caso primero debemos verificar que el valor no sea nulo antes de continuar
        if (value == null)
        {
            // Si es así, devuelve el error requerido
            return new ValidationResult("Email is required!");
        }

        // Esto nos conectará a nuestra base de datos ya que no estamos en nuestro Controlador
        MyContext _context = (MyContext)validationContext.GetService(typeof(MyContext));
        // Verificamos si hay algún registro de este correo electrónico en nuestra base de datos
        if (_context.Users.Any(e => e.Email == value.ToString()))
        {
            // Si es así, arroja un error
            return new ValidationResult("Email must be unique!");
        }
        else
        {
            // Si no, continúa
            return ValidationResult.Success;
        }
    }
}
