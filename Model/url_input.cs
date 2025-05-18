using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;//**************** 
using System.ComponentModel.DataAnnotations.Schema;
namespace project_graduation.Model
{
    public class url_input
    {
        [Key]
        public int id { get; set; }

        public string Url { get; set; }

        public DateTime date { get; set; }

        public int userid { get; set; }

        [ForeignKey(nameof(userid))]
        public virtual users Users { get; set; }
    }
}
