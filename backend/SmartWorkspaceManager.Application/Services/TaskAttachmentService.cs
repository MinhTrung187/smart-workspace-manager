using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Application.Services
{
    public class TaskAttachmentService : ITaskAttachmentService
    {
        private readonly IGenericRepository<TaskAttachment> _attachmentRepository;
        private readonly IGenericRepository<BoardTask> _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;
        private readonly IFileStorageService _fileStorageService;

        public TaskAttachmentService(
            IGenericRepository<TaskAttachment> attachmentRepository,
            IGenericRepository<BoardTask> taskRepository,
            IUserRepository userRepository,
            IUserContext userContext,
            IFileStorageService fileStorageService)
        {
            _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        }

        public async Task<TaskAttachmentDto> AddAttachmentAsync(Guid taskId, System.IO.Stream fileStream, string fileName, string contentType)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var tasks = await _taskRepository.FindAsync(t => t.Id == taskId, "Creator");
            var task = tasks.FirstOrDefault();
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            // Build storage path
            var uniqueName = $"{Guid.NewGuid():N}_{fileName}";
            var path = $"tasks/{taskId}/{uniqueName}";

            var uploadResult = await _fileStorageService.UploadFileAsync(fileStream, path, contentType);

            var attachment = new TaskAttachment
            {
                TaskId = taskId,
                UploadedBy = userId.Value,
                FileName = uploadResult.FileName,
                FileUrl = uploadResult.FileUrl,
                FileSize = uploadResult.FileSize,
                ContentType = uploadResult.ContentType
            };

            await _attachmentRepository.AddAsync(attachment);
            await _attachmentRepository.SaveChangesAsync();

            var uploader = await _userRepository.GetByIdAsync(userId.Value);
            var uploaderName = uploader?.FullName ?? string.Empty;

            return new TaskAttachmentDto(
                attachment.Id,
                attachment.TaskId,
                attachment.FileName,
                attachment.FileUrl,
                attachment.FileSize,
                attachment.ContentType,
                attachment.UploadedBy,
                uploaderName,
                attachment.CreatedAt
            );
        }

        public async Task<IReadOnlyList<TaskAttachmentDto>> GetAttachmentsByTaskAsync(Guid taskId)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var attachments = await _attachmentRepository.FindAsync(a => a.TaskId == taskId, "Task");
            var list = attachments.ToList();

            var userIds = list.Select(a => a.UploadedBy).Distinct().ToList();
            var users = new Dictionary<Guid, Domain.Entities.User>();
            foreach (var id in userIds)
            {
                var u = await _userRepository.GetByIdAsync(id);
                if (u != null) users[id] = u;
            }

            return list.Select(a => new TaskAttachmentDto(
                a.Id,
                a.TaskId,
                a.FileName,
                a.FileUrl,
                a.FileSize,
                a.ContentType,
                a.UploadedBy,
                users.TryGetValue(a.UploadedBy, out var uu) ? uu.FullName : string.Empty,
                a.CreatedAt
            )).ToList();
        }

        public async Task DeleteAttachmentAsync(Guid id)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var attachments = await _attachmentRepository.FindAsync(a => a.Id == id, "Task");
            var attachment = attachments.FirstOrDefault();
            if (attachment == null)
                throw new KeyNotFoundException("Attachment not found.");

            // permission: uploader or task creator or workspace owner could delete (keep it simple: uploader only)
            if (attachment.UploadedBy != userId.Value)
                throw new UnauthorizedAccessException("Only the uploader can delete this attachment.");

            // Delete from storage - derive path from FileUrl is implementation dependent.
            // If storage path was "tasks/{taskId}/{uniqueName}", attempt to remove that prefix from FileUrl.
            // For Supabase implementation below we stored public url as /storage/v1/object/public/{bucket}/{path}
            try
            {
                // attempt to extract path from FileUrl (last segments after bucket)
                var uri = new Uri(attachment.FileUrl);
                // pathSegments: .../public/{bucket}/{path}
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                // find "public" then bucket then remaining path
                var idx = Array.IndexOf(segments, "public");
                if (idx >= 0 && segments.Length > idx + 2)
                {
                    var bucket = segments[idx + 1];
                    var pathParts = segments.Skip(idx + 2);
                    var path = string.Join("/", pathParts);
                    await _fileStorageService.DeleteFileAsync(path);
                }
            }
            catch
            {
                // ignore storage delete errors, continue to remove DB record
            }

            _attachmentRepository.Delete(attachment);
            await _attachmentRepository.SaveChangesAsync();
        }
    }
}
