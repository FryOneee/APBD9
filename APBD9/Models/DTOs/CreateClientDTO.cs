// Models/DTOs/CreateClientDTO.cs
using System.ComponentModel.DataAnnotations;

namespace APBD9.Models.DTOs
{
    public class CreateClientDTO
    {
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required, Phone] public string Telephone { get; set; }
        [Required, RegularExpression(@"^\d{11}$")] public string Pesel { get; set; }
    }
}