using Amazon.S3;
using Amazon.S3.Model;
using Community.API.Common.Helpers;
using Microsoft.Extensions.Options;

namespace Community.API.Services.S3;

public class S3Service : IS3Service
{
    private readonly S3MinioOptions _options;
    private readonly IAmazonS3 _s3Client;

    public S3Service(
        IOptions<S3MinioOptions> options,
        IAmazonS3 s3Client)
    {
        _options = options.Value;
        _s3Client = s3Client;
    }

    public string GetPublicUrl(string objectKey)
    {
        return $"{_options.PublicEndpoint.TrimEnd('/')}/{_options.BucketName}/{objectKey}";
    }

    public string GetPreSignedGetUrl(string objectKey)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };

        return _s3Client.GetPreSignedURL(request);
    }

    public (string UploadUrl, string ObjectKey) GetPreSignedPutUrl(
        Guid articleId,
        string contentType)
    {
        var extension = contentType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            _ => throw new NotSupportedException(
                $"Content type '{contentType}' is not supported.")
        };

        var objectKey =
            $"article/{articleId}.{extension}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        var uploadUrl = _s3Client.GetPreSignedURL(request);
        uploadUrl = uploadUrl.Replace("https://", "http://");
        return (uploadUrl, objectKey);
    }
}