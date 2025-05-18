using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;//**************** 
using System.ComponentModel.DataAnnotations.Schema;
namespace project_graduation.Model
{
    public class data
    {
        [Key]
        public int Id { get; set; }

        public string Results { get; set; }

        public DateTime date { get; set; }

        // مفتاح خارجي
        public int userid { get; set; }

        [ForeignKey(nameof(userid))]
        public virtual users Users { get; set; }
    }
}
