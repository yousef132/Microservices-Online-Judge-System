namespace Community.API.Services.S3;

public interface IS3Service
{
    string GetPreSignedGetUrl(string objectKey);
    (string UploadUrl, string ObjectKey) GetPreSignedPutUrl(Guid articleId, string contentType);
    string GetPublicUrl(string objectKey);
    Task<bool> BucketExistsAsync(string bucketName);
    Task<bool> CreateBucketWithObjectLockAndPublicAccess(string bucketName, bool enableObjectLock);
}
