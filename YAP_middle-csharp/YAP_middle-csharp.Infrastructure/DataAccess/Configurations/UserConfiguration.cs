using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Infrastructure.DataAccess.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserModel>
    {
        public static readonly Guid SystemUserId = new("11111111-1111-1111-1111-111111111111");

        public void Configure(EntityTypeBuilder<UserModel> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Login)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(x => x.Login)
                .IsUnique();

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.UserRole)
                .IsRequired()
                .HasConversion<string>();

            builder.HasData(new UserModel(
                SystemUserId,
                "system_legacy_user",
                "legacy_hash",
                UserRoleEnum.User
            ));
        }
    }
}
