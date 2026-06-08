using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;

namespace SmartWorkspaceManager.Infrastructure.Storage
{
    public class SupabaseFileStorageService : IFileStorageService
    {
        private readonly HttpClient _http;
        private readonly SupabaseStorageOptions _options;

        public SupabaseFileStorageService(HttpClient http, IOptions<SupabaseStorageOptions> options)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<FileUploadResult> UploadFileAsync(Stream stream, string path, string contentType)
        {
            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
                throw new InvalidOperationException("Supabase BaseUrl is not configured.");

            var bucket = _options.Bucket.Trim('/');
            var uploadUrl = new Uri(new Uri(_options.BaseUrl), $"/storage/v1/object/{bucket}/{Uri.EscapeDataString(path)}");

            var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
            request.Content = new StreamContent(stream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

            var key = string.IsNullOrWhiteSpace(_options.ServiceKey) ? _options.AnonKey : _options.ServiceKey;
            request.Headers.Add("apikey", _options.AnonKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);

            var resp = await _http.SendAsync(request);
            resp.EnsureSuccessStatusCode();

            // Public url pattern
            var publicUrl = new Uri(new Uri(_options.BaseUrl), $"/storage/v1/object/public/{bucket}/{Uri.EscapeDataString(path)}").ToString();

            // attempt to get content length
            long length = stream.CanSeek ? stream.Length : 0;
            if (resp.Content.Headers.ContentLength.HasValue)
                length = resp.Content.Headers.ContentLength.Value;

            return new FileUploadResult(publicUrl, Path.GetFileName(path), length, contentType);
        }

        public async Task DeleteFileAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
                throw new InvalidOperationException("Supabase BaseUrl is not configured.");

            var bucket = _options.Bucket.Trim('/');
            var deleteUrl = new Uri(new Uri(_options.BaseUrl), $"/storage/v1/object/{bucket}/{Uri.EscapeDataString(path)}");

            var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            var key = string.IsNullOrWhiteSpace(_options.ServiceKey) ? _options.AnonKey : _options.ServiceKey;
            request.Headers.Add("apikey", _options.AnonKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);

            var resp = await _http.SendAsync(request);
            resp.EnsureSuccessStatusCode();
        }
    }
}
