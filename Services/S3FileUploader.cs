using System.Net;
using Amazon.S3;
using Amazon.S3.Model;

namespace WebAdvert.Web.Services
{
    public class S3FileUploader : IFileUploader
    {
        private readonly IAmazonS3 _amazonS3;
        private readonly IConfiguration _configuration;

        public S3FileUploader(IAmazonS3 amazonS3, IConfiguration configuration)
        {
            _amazonS3 = amazonS3;
            _configuration = configuration;
        }

        public async Task<bool> UploadFileAsync(string fileName, Stream storageStream)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var bucketName = _configuration.GetValue<string>("ImageBucket");

            if (storageStream.Length>0)
                if (storageStream.CanSeek)
                    storageStream.Seek(0, SeekOrigin.Begin);

            var putRequest = new PutObjectRequest()
            {
                AutoCloseStream = true,
                BucketName = bucketName,
                InputStream = storageStream,
                Key = fileName
            };

            var response = await _amazonS3.PutObjectAsync(putRequest);
            return response.HttpStatusCode == HttpStatusCode.OK;
        }
    }
}
