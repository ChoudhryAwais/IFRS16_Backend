namespace IFRS16_Backend.Models
{
    public class Login
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }

    public class LoginResponse
    {
        public User UserInfo { get; set; }
        public CompanyProfile CompanyProfile { get; set; }
    }
}
