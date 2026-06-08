using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface ITaskAttachmentService
    {
        Task<TaskAttachmentDto> AddAttachmentAsync(Guid taskId, Stream fileStream, string fileName, string contentType);
        Task<IReadOnlyList<TaskAttachmentDto>> GetAttachmentsByTaskAsync(Guid taskId);
        Task DeleteAttachmentAsync(Guid id);
    }
}
