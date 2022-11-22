using System.ComponentModel.DataAnnotations;

namespace Backend.Model
{
    public class LoginModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}