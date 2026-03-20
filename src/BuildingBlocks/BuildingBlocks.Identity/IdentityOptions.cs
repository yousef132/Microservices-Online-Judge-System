namespace BuildingBlocks.Identity;

public class IdentityOptions
{
    public string AdminUrl { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;
    public string ConfidentialClientId { get; set; } = string.Empty;
    public string ConfidentialClientSecret { get; set; } = string.Empty;
    public string PublicClientId { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
}