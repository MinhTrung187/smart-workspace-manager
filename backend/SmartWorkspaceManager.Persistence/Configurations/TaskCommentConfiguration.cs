using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
    {
        public void Configure(EntityTypeBuilder<TaskComment> builder)
        {
            builder.ToTable("TaskComments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.TaskId)
                   .IsRequired();

            builder.Property(c => c.UserId)
                   .IsRequired();

            builder.Property(c => c.Content)
                   .IsRequired()
                   .HasMaxLength(4000);

            builder.Property(c => c.CreatedAt)
                   .IsRequired();

            builder.Property(c => c.IsDeleted)
                   .HasDefaultValue(false);

            // Relations
            builder.HasOne(c => c.Task)
                   .WithMany(t => t.Comments)
                   .HasForeignKey(c => c.TaskId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.User)
                   .WithMany()
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Soft-delete global query filter
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
