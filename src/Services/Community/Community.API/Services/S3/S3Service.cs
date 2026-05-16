namespace Community.API.Services.S3;

public class S3Service : IS3Service
{
    private const string BucketName = "online-judge-community-bucket";

    public string GetPreSignedGetUrl(string objectKey) =>
        $"https://{BucketName}.s3.amazonaws.com/{objectKey}?presigned_placeholder=true";

    public (string UploadUrl, string ObjectKey) GetPreSignedPutUrl(Guid articleId, string contentType)
    {
        var extension = contentType switch
        {
            "image/jpeg" => "jpg",
            "image/png"  => "png",
            "image/webp" => "webp",
            _ => "tmp"
        };
        var objectKey = $"articles/{articleId}/cover.{extension}";
        var uploadUrl = $"https://{BucketName}.s3.amazonaws.com/{objectKey}?presigned_put_placeholder=true";
        return (uploadUrl, objectKey);
    }
}
