using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
    {
        public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
        {
            builder.ToTable("WorkspaceMembers");

            builder.HasKey(wm => new { wm.UserId, wm.WorkspaceId });

            builder.Property(wm => wm.Role)
                   .IsRequired();

            builder.Property(wm => wm.JoinedAt)
                   .IsRequired()
                   .HasDefaultValueSql("now()");

            builder.HasOne(wm => wm.User)
                   .WithMany(u => u.WorkspaceMemberships)
                   .HasForeignKey(wm => wm.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(wm => wm.Workspace)
                   .WithMany(w => w.Members)
                   .HasForeignKey(wm => wm.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(wm => wm.WorkspaceId);
            builder.HasQueryFilter(x => !x.User.IsDeleted);


        }
    }
}
