using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Store.Data.DTO
{
    public class RegisterDTO
    {
        [Required]
        [EmailAddress]
        public string EmailAdress { get; set; } = string.Empty;
        [Required]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
