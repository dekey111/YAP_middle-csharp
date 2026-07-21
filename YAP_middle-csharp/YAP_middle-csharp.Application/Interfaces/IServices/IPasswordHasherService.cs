using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp.Application.Interfaces.IServices
{
    public interface IPasswordHasherService
    {
        string HasPassword(string password);
        bool CheckPassword(string password, string hasPassword);
    }
}
