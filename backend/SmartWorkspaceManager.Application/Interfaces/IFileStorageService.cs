    using System.IO;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IFileStorageService
    {

        Task<FileUploadResult> UploadFileAsync(Stream stream, string path, string contentType);


        Task DeleteFileAsync(string path);
    }
}
