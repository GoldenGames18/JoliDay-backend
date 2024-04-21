using JoliDay.Config;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JoliDay.Models
{
    public class JoliDayContext : IdentityDbContext<User>
    {
        public JoliDayContext(DbContextOptions<JoliDayContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new HolidayConfiguration());
            base.OnModelCreating(builder);
        }

        public DbSet<Activity> Activitys { get; set; }
        public DbSet<Address> Addresss { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
    }
}