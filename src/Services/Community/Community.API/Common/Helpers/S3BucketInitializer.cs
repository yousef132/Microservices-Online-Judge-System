using Community.API.Services.S3;

namespace Community.API.Common.Helpers
{
    public sealed class S3BucketInitializer
    {
        private readonly IS3Service _s3;

        public S3BucketInitializer(IS3Service s3)
        {
            _s3 = s3;
        }

        public async Task EnsurePublicBucketAsync(
            string bucketName,
            CancellationToken cancellationToken = default)
        {
            var exists = await _s3.BucketExistsAsync(bucketName);

            if (!exists)
            {
                await _s3.CreateBucketWithObjectLockAndPublicAccess(
                    bucketName,
                    false);
            }
        }
    }
}
