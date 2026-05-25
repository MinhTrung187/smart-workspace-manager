using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class TaskAssigneeConfiguration : IEntityTypeConfiguration<TaskAssignee>
    {
        public void Configure(EntityTypeBuilder<TaskAssignee> builder)
        {
            builder.ToTable("TaskAssignees");

            // Composite primary key
            builder.HasKey(a => new { a.TaskId, a.UserId });

            // Relations
            builder.HasOne(a => a.Task)
                   .WithMany(t => t.Assignees)
                   .HasForeignKey(a => a.TaskId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();

            builder.HasOne(a => a.User)
                   .WithMany()
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();

            // Indexes for efficient lookups
            builder.HasIndex(a => a.TaskId);
            builder.HasIndex(a => a.UserId);
            builder.HasQueryFilter(x => !x.Task.IsDeleted); // Soft delete filters
        }
    }
}
