using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class BoardConfiguration : IEntityTypeConfiguration<Board>
    {
        public void Configure(EntityTypeBuilder<Board> builder)
        {
            builder.ToTable("Boards");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(b => b.WorkspaceId)
                   .IsRequired();

            builder.Property(b => b.CreatedBy)
                   .IsRequired();

            builder.Property(b => b.CreatedAt)
                   .IsRequired();

            builder.Property(b => b.IsDeleted)
                   .HasDefaultValue(false);

            // Relations
            builder.HasOne(b => b.Workspace)
                   .WithMany()
                   .HasForeignKey(b => b.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Creator)
                   .WithMany()
                   .HasForeignKey(b => b.CreatedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(b => b.WorkspaceId);
            builder.HasIndex(b => b.CreatedBy);

            // Soft-delete global query filter
            builder.HasQueryFilter(b => !b.IsDeleted);
        }
    }
}
