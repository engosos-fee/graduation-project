namespace project_graduation.DTOclass
{
    // DTO used for resetting a user's password
    public class ResetPasswordDto
    {
        // The password reset token sent to the user's email
        public string Token { get; set; }

        // The new password entered by the user
        public string NewPassword { get; set; }
    }
}
