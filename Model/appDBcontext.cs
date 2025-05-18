using Microsoft.EntityFrameworkCore;

namespace project_graduation.Model
{
    public class appDBcontext : DbContext
    {
        public appDBcontext(DbContextOptions<appDBcontext> options)
            : base(options)
        {
        }

        public virtual DbSet<data> data { get; set; }
        public virtual DbSet<users> Users { get; set; }
        public virtual DbSet<url_input> url_Input { get; set; }
    }
}
