namespace TitleVerification.Api.DTOs.Auth
{
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AadhaarNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
