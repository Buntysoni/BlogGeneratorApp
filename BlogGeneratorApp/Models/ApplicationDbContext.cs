using Microsoft.EntityFrameworkCore;

namespace BlogGeneratorApp.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }
    }
}
