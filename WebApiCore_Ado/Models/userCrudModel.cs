using System.ComponentModel.DataAnnotations;

namespace WebApiCore_Ado.Models
{
     
        public class addUserDTO
        { 
            public string? name { get; set; }

            [Required, EmailAddress]
            public string? email { get; set; }

            [Required]
            public string? pass { get; set; }
            public IFormFile? img { get; set; }
            public string? gender { get; set; }
            public List<string>? hobby { get; set; }
            public string? address { get; set; }
            public string? profession { get; set; } 
            public DateOnly? dob { get; set; } 
        }
      
        public class EditUserDTO
        {
            [Required]
            public int? id { get; set; }  

            [Required]
            public string? name { get; set; }  
            public IFormFile? img { get; set; }
            public string? gender { get; set; }
            public List<string>? hobby { get; set; }
            public string? address { get; set; }
            public string? profession { get; set; } 
            public bool? isActive { get; set; } 
            public string? role { get; set; } 
            public DateOnly? dob { get; set; } 
        }

        public class GetUserDTO
        {
            public int Id { get; set; }
            public string? name { get; set; } 
            public string? email { get; set; }
            public string? img { get; set; }
            public string? gender { get; set; }
            public List<string>? hobby { get; set; }
            public string? address { get; set; }
            public string? profession { get; set; }
            public string? isActive { get; set; }
            public string? dob { get; set; }
            public string? role { get; set; }
        }
       
    
}
