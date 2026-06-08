using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class TaskAttachmentConfiguration : IEntityTypeConfiguration<TaskAttachment>
    {
        public void Configure(EntityTypeBuilder<TaskAttachment> builder)
        {
            builder.ToTable("TaskAttachments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.FileName)
                   .IsRequired()
                   .HasMaxLength(1024);

            builder.Property(a => a.FileUrl)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(a => a.FileSize)
                   .IsRequired();

            builder.Property(a => a.ContentType)
                   .HasMaxLength(200);

            builder.Property(a => a.UploadedBy)
                   .IsRequired();

            builder.Property(a => a.CreatedAt)
                   .IsRequired();

            // Relations
            builder.HasOne(a => a.Task)
                   .WithMany(t => t.Attachments)
                   .HasForeignKey(a => a.TaskId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Soft-delete query filter (BaseEntity expected to have IsDeleted)
            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
