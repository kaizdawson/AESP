namespace AESP.Common.DTOs
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }

        public string? RoleName { get; set; }

        public bool? IsPlacementTestDone { get; set; }
        public bool? IsGoalSet { get; set; }
        public bool? IsProfileCompleted { get; set; }
    }
}
