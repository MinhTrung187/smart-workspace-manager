using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.Property(u => u.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(u => u.FullName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(u => u.AvatarUrl)
                   .HasMaxLength(500);

            builder.Property(u => u.CreatedAt)
                   .IsRequired();

            builder.Property(u => u.IsDeleted)
                   .HasDefaultValue(false);

            // Relations
            builder.HasMany(u => u.OwnedWorkspaces)
                   .WithOne(w => w.Owner)
                   .HasForeignKey(w => w.OwnerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.WorkspaceMemberships)
                   .WithOne(wm => wm.User)
                   .HasForeignKey(wm => wm.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Soft-delete global query filter
            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
