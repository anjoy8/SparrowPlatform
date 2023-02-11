using SparrowPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SparrowPlatform.Infrastruct.Mappings
{
    public class UserInfoMap : IEntityTypeConfiguration<UserInfo>
    {
        public void Configure(EntityTypeBuilder<UserInfo> builder)
        {
            builder.Property(c => c.Login)
                .HasColumnName("login")
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Password)
                .HasColumnName("password")
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.DisplayName)
                .HasColumnName("display_name")
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.DataScope)
                .HasColumnName("data_scope")
                .IsRequired(); 
            builder.Property(c => c.Validity)
                .HasColumnName("validity")
                .HasColumnType("datetime");

            builder.Property(c => c.Email)
                .HasColumnName("email")
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100);

            builder.Property(c => c.ApplicationScopeAll)
                .HasColumnName("applicationScopeAll")
                .HasColumnType("nvarchar(500)")
                .HasMaxLength(500);

            builder.Property(c => c.Remark)
                .HasColumnName("remark")
                .HasColumnType("nvarchar(2000)")
                .HasMaxLength(2000);

            builder.Property(c => c.AADId)
                .HasColumnName("aad_id")
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(c => c.IsDeleted)
                .HasColumnName("is_deleted")
                .IsRequired();
        }
    }
}
