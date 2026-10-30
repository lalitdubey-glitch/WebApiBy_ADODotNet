using Microsoft.AspNetCore.Antiforgery;
using System.ComponentModel.DataAnnotations;

namespace WebApiCore_Ado.Models
{
    public class loginDTO
    {
        [Required]
        [EmailAddress]
        public string? email { get; set; }

        [Required]
        public string? pass { get; set; }
    }
}
