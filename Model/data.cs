using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project_graduation.Model
{
    // Represents scan result data submitted by a user
    public class data
    {
        // Primary key
        [Key]
        public int Id { get; set; }

        // The scan results as a string
        public string Results { get; set; }

        // Date when the result was created
        public DateTime date { get; set; }

        // Foreign key referencing the user who submitted the data
        public int userid { get; set; }

        // Navigation property linking to the user entity
        [ForeignKey(nameof(userid))]
        public virtual users Users { get; set; }
    }
}
