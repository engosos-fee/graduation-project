using System.ComponentModel.DataAnnotations;//**************** 
using System.ComponentModel.DataAnnotations.Schema;
namespace project_graduation.Model
{
    public class users
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public char role { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? TokenExpirationTime { get; set; }
        public int FailedLoginAttempts { get; set; } = 0; // عدد المحاولات الفاشلة
        public DateTime? LockoutEnd { get; set; } // الوقت الذي ينتهي فيه القفل
        public DateTime? LastConfirmationEmailSent { get; set; }
        public virtual List<data> datas { get; set; }
        public virtual List<url_input> Url_Inputs { get; set; }
    }
}
