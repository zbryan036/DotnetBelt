using Microsoft.EntityFrameworkCore;

namespace DotnetBelt.Models {
    public class BeltContext: DbContext {
        public BeltContext(DbContextOptions options): base(options) {}
        public DbSet<Occurrence> Occurrences { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Plan> Plans { get; set; }
    }
}