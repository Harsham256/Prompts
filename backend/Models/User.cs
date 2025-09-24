namespace TitleVerification.Api.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty; // unique
        public string PasswordHash { get; set; } = string.Empty;
        public string AadhaarNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}