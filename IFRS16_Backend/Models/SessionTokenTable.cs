namespace IFRS16_Backend.Models
{
    public class SessionTokenTable
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
    public class UpsertRequest
    {
        public int UserId { get; set; }
        public string? Token { get; set; } = string.Empty;
    }
}
