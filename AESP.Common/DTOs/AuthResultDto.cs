namespace AESP.Common.DTOs
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? RoleName { get; set; }
        public string? Email { get; set; }
    }
}
