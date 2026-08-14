using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YAP_middle_csharp_Auth.Application.Models
{
    public class LoginRequest
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов!")]
        public required string Login { get; set; } = string.Empty;

        [StringLength(20, MinimumLength = 5, ErrorMessage = "Пароль должен быть от 5 до 20 символов!")]
        public required string Password { get; set; } = string.Empty;
    }
}
