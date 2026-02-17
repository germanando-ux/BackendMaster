using System;
using System.Collections.Generic;
using System.Text;

namespace Store.Domain.Models
{
    /// <summary>
    /// Clase de la tabla de usuarios para autenticación y autorización.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        // Guardaremos el hash, no la clave plana por seguridad 🛡️
        public byte[] PasswordHash { get; set; } = [];
        public byte[] PasswordSalt { get; set; } = [];

        // Para los Claims que mencionamos antes
        public string Role { get; set; } = "Seller";
    }
}
