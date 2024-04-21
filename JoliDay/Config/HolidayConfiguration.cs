using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using JoliDay.Models;

namespace JoliDay.Config
{
    public class HolidayConfiguration : IEntityTypeConfiguration<Models.Holiday>
    {
        public void Configure(EntityTypeBuilder<Models.Holiday> builder)
        {
            builder.HasOne(r => r.Owner)
               .WithMany()
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany<User>(r => r.Users)
                   .WithMany();
                   
        }


    }
}
