using System;

namespace SmartWorkspaceManager.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }

        public bool IsDeleted { get; protected set; } = false;
        public DateTime? DeletedAt { get; protected set; }

        public void Touch() => UpdatedAt = DateTime.UtcNow;

        public void SoftDelete()
        {
            if (IsDeleted) return;
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            Touch();
        }

        public void Restore()
        {
            if (!IsDeleted) return;
            IsDeleted = false;
            DeletedAt = null;
            Touch();
        }
    }
}
