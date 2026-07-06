using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<BookingModel>
    {
        public void Configure(EntityTypeBuilder<BookingModel> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.Property(x => x.ProcessedAt);

            builder.HasOne(x => x.Event)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.EventId)
                .IsRequired();
        }
    }
}
