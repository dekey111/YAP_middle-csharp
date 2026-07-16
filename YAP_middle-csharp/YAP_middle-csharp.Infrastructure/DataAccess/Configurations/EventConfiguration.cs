using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Infrastructure.DataAccess.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<EventModel>
    {
        public void Configure(EntityTypeBuilder<EventModel> builder)
        {
            builder.ToTable("Events");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.Description)
                .HasMaxLength(250);

            builder.Property(x => x.TotalSeats).IsRequired();
            builder.Property(x => x.AvailableSeats).IsRequired();
            builder.Property(x => x.StartAt).IsRequired();
            builder.Property(x => x.EndAt).IsRequired();

            builder.HasMany(x => x.Bookings)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId);
        }
    }
}
