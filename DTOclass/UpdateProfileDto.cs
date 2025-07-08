namespace project_graduation.DTOclass
{
    // DTO used for updating user profile information
    public class UpdateProfileDto
    {
        // New name for the user (optional)
        public string? NewName { get; set; }

        // New email address for the user (optional)
        public string? NewEmail { get; set; }

        // Current password (required when changing password)
        public string? CurrentPassword { get; set; }

        // New password (optional, requires current password)
        public string? NewPassword { get; set; }
    }
}
