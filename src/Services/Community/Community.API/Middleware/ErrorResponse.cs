namespace Community.API.Middleware;

public class ErrorResponse
{
    public int Status { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Errors { get; set; }
}
