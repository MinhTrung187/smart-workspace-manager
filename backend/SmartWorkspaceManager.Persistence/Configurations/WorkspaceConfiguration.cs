using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.ToTable("Workspaces");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(w => w.Description)
                   .HasMaxLength(1000);

            builder.Property(w => w.OwnerId)
                   .IsRequired();

            builder.Property(w => w.CreatedAt)
                   .IsRequired();

            builder.Property(w => w.IsDeleted)
                   .HasDefaultValue(false);

            // Owner relationship (matches configuration in UserConfiguration)
            builder.HasOne(w => w.Owner)
                   .WithMany(u => u.OwnedWorkspaces)
                   .HasForeignKey(w => w.OwnerId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Members navigation
            builder.HasMany(w => w.Members)
                   .WithOne(m => m.Workspace)
                   .HasForeignKey(m => m.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Soft-delete global query filter
            builder.HasQueryFilter(w => !w.IsDeleted);
        }
    }
}
