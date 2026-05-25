using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class BoardTaskConfiguration : IEntityTypeConfiguration<BoardTask>
    {
        public void Configure(EntityTypeBuilder<BoardTask> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(t => t.Description)
                   .HasMaxLength(4000);

            builder.Property(t => t.ColumnId)
                   .IsRequired();

            builder.Property(t => t.Priority)
                   .IsRequired()
                   .HasConversion<int>();

            builder.Property(t => t.Position)
                   .IsRequired();

            builder.Property(t => t.CreatedBy)
                   .IsRequired();

            builder.Property(t => t.CreatedAt)
                   .IsRequired();

            builder.Property(t => t.IsDeleted)
                   .HasDefaultValue(false);

            // Relations
            builder.HasOne(t => t.Column)
                   .WithMany()
                   .HasForeignKey(t => t.ColumnId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Creator)
                   .WithMany()
                   .HasForeignKey(t => t.CreatedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            // Assignees relation
            builder.HasMany(t => t.Assignees)
                   .WithOne(a => a.Task)
                   .HasForeignKey(a => a.TaskId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.ColumnId);
            builder.HasIndex(t => t.CreatedBy);
            builder.HasIndex(t => new { t.ColumnId, t.Position });

            // Soft-delete global query filter
            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
