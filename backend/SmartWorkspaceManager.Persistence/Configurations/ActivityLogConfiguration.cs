using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.ToTable("ActivityLogs");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.WorkspaceId)
                   .IsRequired();

            builder.Property(a => a.UserId)
                   .IsRequired();

            builder.Property(a => a.ActivityType)
                   .IsRequired();

            builder.Property(a => a.Description)
                   .HasMaxLength(2000);

            builder.Property(a => a.CreatedAt)
                   .IsRequired();

            builder.Property(a => a.IsDeleted)
                   .HasDefaultValue(false);

            // Relations
            builder.HasOne(a => a.Workspace)
                   .WithMany()
                   .HasForeignKey(a => a.WorkspaceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.User)
                   .WithMany()
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Task)
                   .WithMany()
                   .HasForeignKey(a => a.TaskId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes for query performance
            builder.HasIndex(a => a.WorkspaceId);
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.TaskId);

            // Soft-delete global query filter
            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
