namespace Community.API.Common.Helpers
{
    public class S3MinioOptions
    {
        public string Endpoint { get; set; } = default!;
        public string PublicEndpoint { get; set; } = default!;
        public string AccessKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public string BucketName { get; set; } = default!;
        public bool UseSSL { get; set; }
    }
}
