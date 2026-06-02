using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Persistence.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.ChannelId)
                   .IsRequired();

            builder.Property(m => m.UserId)
                   .IsRequired();

            builder.Property(m => m.Content)
                   .IsRequired()
                   .HasMaxLength(4000);

            builder.Property(m => m.CreatedAt)
                   .IsRequired();

            builder.Property(m => m.IsDeleted)
                   .HasDefaultValue(false);

            builder.Property(m => m.EditedAt)
                   .IsRequired(false);

            // Relations
            builder.HasOne(m => m.Channel)
                   .WithMany(c => c.Messages)
                   .HasForeignKey(m => m.ChannelId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();

            builder.HasOne(m => m.User)
                   .WithMany()
                   .HasForeignKey(m => m.UserId)
                   // keep messages when user is removed by using Restrict
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            // Indexes
            builder.HasIndex(m => m.ChannelId);
            builder.HasIndex(m => m.UserId);
            builder.HasIndex(m => m.CreatedAt);

            // Soft-delete filter
            builder.HasQueryFilter(m => !m.IsDeleted);
        }
    }
}
