using Microsoft.EntityFrameworkCore;

namespace project_graduation.Model
{
    // The main database context for the application
    public class appDBcontext : DbContext
    {
        public appDBcontext(DbContextOptions<appDBcontext> options)
            : base(options)
        {
        }

        // Table for storing scan result data
        public virtual DbSet<data> data { get; set; }

        // Table for storing user information
        public virtual DbSet<users> Users { get; set; }

        // Table for storing URLs submitted by users
        public virtual DbSet<url_input> url_Input { get; set; }
    }
}
