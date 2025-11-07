namespace IFRS16_Backend.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string UserAddress { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int CompanyID { get; set; }
        public string Role { get; set; }
        public string? CurrentSessionToken { get; set; } // Make nullable to allow assignment of null


    }
    public class UserPasswordDto
    {
        public int UserId { get; set; }
        public string Password { get; set; }
    }
}
