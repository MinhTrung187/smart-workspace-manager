using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.Infrastructure.Storage
{
    public sealed class SupabaseStorageOptions
    {
        public string BaseUrl { get; set; } = string.Empty; 
        public string AnonKey { get; set; } = string.Empty;
        public string ServiceKey { get; set; } = string.Empty;
        public string Bucket { get; set; } = "public";
        public string? FolderPrefix { get; set; }
    }
}
