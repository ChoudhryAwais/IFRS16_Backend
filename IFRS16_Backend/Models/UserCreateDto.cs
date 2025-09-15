namespace IFRS16_Backend.Models
{
    public class UserCreateDto
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string UserAddress { get; set; }
        public string Email { get; set; }
        public int CompanyID { get; set; }
        public string Role { get; set; }
    }
}
