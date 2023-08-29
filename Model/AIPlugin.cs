using System.Text.Json.Serialization;

public class AIPlugin
{
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "v1";

    [JsonPropertyName("name_for_model")]
    public string NameForModel { get; set; } = "azblob";

    [JsonPropertyName("name_for_human")]
    public string NameForHuman { get; set; } = "azblob";

    [JsonPropertyName("description_for_model")]
    public string DescriptionForModel { get; set; } = "Creates writeable azure blobs.";

    [JsonPropertyName("description_for_human")]
    public string DescriptionForHuman { get; set; } = "Creates Block, Page or Append blobs on Azure";

    public AIPluginAuth Auth { get; set; } = new AIPluginAuth { Type = "none" };

    public AIPluginAPI Api { get; set; } = new AIPluginAPI { Type = "openapi" };

    [JsonPropertyName("contact_email")]
    public string ContactEmail { get; set; } = string.Empty;

    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;

    [JsonPropertyName("legal_info_url")]
    public string LegalInfoUrl { get; set; } = string.Empty;
}

public class AIPluginAuth
{
    public string Type { get; set; } = string.Empty;
}

public class AIPluginAPI
{
    public string Type { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}