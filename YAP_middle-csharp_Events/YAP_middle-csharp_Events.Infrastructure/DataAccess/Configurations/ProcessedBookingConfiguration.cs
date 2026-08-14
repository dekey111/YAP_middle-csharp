using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp_Events.Domain.Models;

namespace YAP_middle_csharp_Events.Infrastructure.DataAccess.Configurations
{
    public class ProcessedBookingConfiguration : IEntityTypeConfiguration<ProcessedBookingsModel>
    {
        public void Configure(EntityTypeBuilder<ProcessedBookingsModel> builder)
        {
            builder.ToTable("ProcessedBookings");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.BookingId).IsRequired();
            builder.Property(x => x.ProcessedAt).IsRequired();
        }
    }
}
