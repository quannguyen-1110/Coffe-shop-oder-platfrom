using Amazon.S3;
using Amazon.S3.Model;

namespace CoffeeShopAPI.Services
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string bucketName, string keyName);
        Task<bool> DeleteFileAsync(string bucketName, string keyName);
        Task<string> GeneratePreSignedUrlAsync(string bucketName, string keyName, TimeSpan expiry);
    }

    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;

        public S3Service(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string bucketName, string keyName)
        {
            try
            {
                using var stream = file.OpenReadStream();
                
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    InputStream = stream,
                    ContentType = file.ContentType,
                    
                };

                var response = await _s3Client.PutObjectAsync(request);
                
                return $"https://{bucketName}.s3.amazonaws.com/{keyName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file to S3: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string bucketName, string keyName)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                await _s3Client.DeleteObjectAsync(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GeneratePreSignedUrlAsync(string bucketName, string keyName, TimeSpan expiry)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = keyName,
                Expires = DateTime.UtcNow.Add(expiry),
                Verb = HttpVerb.GET
            };

            return await _s3Client.GetPreSignedURLAsync(request);
        }
    }
}