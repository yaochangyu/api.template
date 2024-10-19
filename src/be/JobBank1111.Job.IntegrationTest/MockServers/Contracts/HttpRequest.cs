using System.Text.Json.Serialization;

namespace JobBank1111.Job.WebAPI.IntegrationTest.MockServers.Contracts;

public class HttpRequest
{
    public string Method { get; set; }
    public string Path { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Cookies Cookies { get; set; }
}