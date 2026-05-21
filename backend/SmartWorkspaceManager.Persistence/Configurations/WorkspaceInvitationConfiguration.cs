using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
    {
        public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
        {
            builder.ToTable("WorkspaceInvitations");

            builder.HasKey(wi => wi.Id);

            builder.Property(wi => wi.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(wi => wi.Status)
                   .IsRequired();

            builder.Property(wi => wi.ExpiredAt)
                   .IsRequired();

            builder.Property(wi => wi.CreatedAt)
                   .IsRequired();

            builder.Property(wi => wi.IsDeleted)
                   .HasDefaultValue(false);

            // Relationship to Workspace (Workspace does not currently expose a navigation collection)
            builder.HasOne(wi => wi.Workspace)
                   .WithMany()
                   .HasForeignKey(wi => wi.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(wi => wi.WorkspaceId);
            // prevent duplicate active invitations for same email in same workspace
            builder.HasIndex(wi => new { wi.WorkspaceId, wi.Email })
                   .IsUnique();

            // Soft-delete global query filter
            builder.HasQueryFilter(wi => !wi.IsDeleted);
        }
    }
}
