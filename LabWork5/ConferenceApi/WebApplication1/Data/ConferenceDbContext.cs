namespace ConferenceApi.Data
{
    using ConferenceApi.Models;
    using Microsoft.EntityFrameworkCore;

    public class ConferenceDbContext : DbContext
    {
        public ConferenceDbContext(DbContextOptions<ConferenceDbContext> options) : base(options) { }

        public DbSet<Conference> Conferences { get; set; } = null!;
        public DbSet<Talk> Talks { get; set; } = null!;
        public DbSet<Speaker> Speakers { get; set; } = null!;
        public DbSet<Vote> Votes { get; set; } = null!;
        public DbSet<User> Users { get; set; }
    }
}
