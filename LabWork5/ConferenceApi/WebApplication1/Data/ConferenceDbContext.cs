namespace ConferenceApi.Data
{
    using ConferenceApi.Models;
    using Microsoft.EntityFrameworkCore;

    public class ConferenceDbContext : DbContext
    {
        public ConferenceDbContext(DbContextOptions<ConferenceDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conference>()
                .HasMany(c => c.Talks)
                .WithOne(t => t.Conference)
                .HasForeignKey(t => t.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade); // <- каскадное удаление

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Conference> Conferences { get; set; } = null!;
        public DbSet<Talk> Talks { get; set; } = null!;
        public DbSet<Speaker> Speakers { get; set; } = null!;
        public DbSet<Vote> Votes { get; set; } = null!;
        public DbSet<User> Users { get; set; }
    }
}
