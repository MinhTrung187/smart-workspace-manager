using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class ChatChannelConfiguration : IEntityTypeConfiguration<ChatChannel>
    {
        public void Configure(EntityTypeBuilder<ChatChannel> builder)
        {
            builder.ToTable("ChatChannels");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.Type)
                   .IsRequired()
                   .HasConversion<int>();

            builder.Property(c => c.WorkspaceId)
                   .IsRequired();

            builder.Property(c => c.CreatedAt)
                   .IsRequired();

            builder.Property(c => c.IsDeleted)
                   .HasDefaultValue(false);

            // Relations
            builder.HasOne(c => c.Workspace)
                   .WithMany()
                   .HasForeignKey(c => c.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Optional relation to task
            builder.HasOne(c => c.Task)
                   .WithMany()
                   .HasForeignKey(c => c.TaskId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(c => c.WorkspaceId);
            builder.HasIndex(c => c.TaskId);

            // Soft-delete filter
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
