using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp.Application.Models
{
    public class LoginRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
