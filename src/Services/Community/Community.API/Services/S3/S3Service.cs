using Amazon.S3;
using Amazon.S3.Model;
using Community.API.Common.Helpers;
using Microsoft.Extensions.Options;
using System.Text.Json;

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
    public async Task<bool> BucketExistsAsync(string bucketName)
    {
        try
        {
            await _s3Client.GetBucketLocationAsync(
                new GetBucketLocationRequest
                {
                    BucketName = bucketName
                });

            return true;
        }
        catch (AmazonS3Exception ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }


    public async Task<bool> CreateBucketWithObjectLockAndPublicAccess(string bucketName, bool enableObjectLock)
    {
        Console.WriteLine($"\tCreating bucket {bucketName} with object lock {enableObjectLock} and public access.");
        try
        {
            // 1.create bucket
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true,
                ObjectLockEnabledForBucket = enableObjectLock,
            };
    
            var putBucketResponse = await _s3Client.PutBucketAsync(putBucketRequest);
    
            if (putBucketResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }
    
            // 2.  (Disable Block Public Access)
            Console.WriteLine("\tDisabling Block Public Access...");
            var publicAccessBlockRequest = new PutPublicAccessBlockRequest
            {
                BucketName = bucketName,
                PublicAccessBlockConfiguration = new PublicAccessBlockConfiguration
                {
                    BlockPublicAcls = false,
                    IgnorePublicAcls = false,
                    BlockPublicPolicy = false,
                    RestrictPublicBuckets = false
                }
            };
    
            await _s3Client.PutPublicAccessBlockAsync(publicAccessBlockRequest);
    
            // 3.(Apply Public Read Policy)
            Console.WriteLine("\tApplying public read bucket policy...");
            var bucketPolicy = new
            {
                Version = "2012-10-17",
                Statement = new[]
                {
                    new
                    {
                        Sid = "PublicReadGetObject",
                        Effect = "Allow",
                        Principal = "*",
                        Action = "s3:GetObject",
                        Resource = $"arn:aws:s3:::{bucketName}/*"
                    }
                }
            };
    
            //JSON
            string policyJson = JsonSerializer.Serialize(bucketPolicy);
    
            var putPolicyRequest = new PutBucketPolicyRequest
            {
                BucketName = bucketName,
                Policy = policyJson
            };
    
            await _s3Client.PutBucketPolicyAsync(putPolicyRequest);
    
            Console.WriteLine($"\tBucket {bucketName} created successfully with public access.");
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"AWS S3 Error: '{ex.Message}'");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: '{ex.Message}'");
            return false;
        }
    }



}