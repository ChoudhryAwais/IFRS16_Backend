namespace IFRS16_Backend.Models
{
    public class UserUpdateDto
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string? PasswordHash { get; set; } // Optional: only update if provided
        public string PhoneNumber { get; set; }
        public string UserAddress { get; set; }
    }
}
