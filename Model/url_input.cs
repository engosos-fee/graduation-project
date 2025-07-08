using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project_graduation.Model
{
    // Represents a URL submitted by a user for scanning
    public class url_input
    {
        // Primary key
        [Key]
        public int id { get; set; }

        // The URL string submitted by the user
        public string Url { get; set; }

        // The date the URL was submitted
        public DateTime date { get; set; }

        // Foreign key referencing the user who submitted the URL
        public int userid { get; set; }

        // Navigation property linking to the user entity
        [ForeignKey(nameof(userid))]
        public virtual users Users { get; set; }
    }
}
