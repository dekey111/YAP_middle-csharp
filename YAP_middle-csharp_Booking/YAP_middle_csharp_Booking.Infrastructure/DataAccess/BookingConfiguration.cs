using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAP_middle_csharp_Booking.Domain.Models;

namespace YAP_middle_csharp_Booking.Infrastructure.DataAccess
{
    public class BookingConfiguration : IEntityTypeConfiguration<BookingModel>
    {
        public void Configure(EntityTypeBuilder<BookingModel> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.EventId)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.SeatsCount)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ProcessedAt);
        }
    }
}
