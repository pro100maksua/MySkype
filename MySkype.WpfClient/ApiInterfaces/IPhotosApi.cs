using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using RestEase;

namespace MySkype.WpfClient.ApiInterfaces
{
    public interface IPhotosApi
    {
        [Header("Authorization")]
        string Token { get; set; }

        [Get("{userId}")]
        Task<Stream> DownloadAsync([Path] Guid userId);

        [Post("{userId}")]
        Task<Guid> UploadAsync([Path] Guid userId, [Body] MultipartFormDataContent content);
    }
}