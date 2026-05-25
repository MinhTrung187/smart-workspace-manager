using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class ColumnConfiguration : IEntityTypeConfiguration<Column>
    {
        public void Configure(EntityTypeBuilder<Column> builder)
        {
            builder.ToTable("Columns");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.BoardId)
                   .IsRequired();

            builder.Property(c => c.Position)
                   .IsRequired();

            builder.Property(c => c.CreatedAt)
                   .IsRequired();

            builder.Property(c => c.IsDeleted)
                   .HasDefaultValue(false);

            // Relations
            builder.HasOne(c => c.Board)
                   .WithMany()
                   .HasForeignKey(c => c.BoardId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(c => c.BoardId);
            builder.HasIndex(c => new { c.BoardId, c.Position });

            // Soft-delete global query filter
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
