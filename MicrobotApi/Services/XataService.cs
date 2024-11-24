using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicrobotApi.Services;

public class XataService
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    public XataService(IConfiguration configuration)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration.GetSection("Xata:Key").Value);
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _baseUrl = "https://Sami-Chkhachkhi-s-workspace-tio4if.eu-west-1.xata.sh/db/microbot:main";
        //_baseUrl = "https://api.xata.io/workspaces";
    }

    public async Task<XataResponse> GetRecordsAsync(string tableName)
    {
        // Define the JSON payload
        var payload = new
        {
            columns = new[] { "xata_id", "name", "runtime_minutes" },
            page = new { size = 200 }
        };

        // Convert the payload to JSON
        var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Make the POST request
        var response = await _client.PostAsync($"{_baseUrl}/tables/{tableName}/query", jsonContent);

        response.EnsureSuccessStatusCode();

        // Read the response content as a string
        var jsonResponse = await response.Content.ReadAsStringAsync();

        // Deserialize the JSON response into the XataResponse model
        var xataResponse = JsonSerializer.Deserialize<XataResponse>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return xataResponse;
    }

    public async Task<string> AddRecordAsync(string tableName, object record)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{_baseUrl}/tables/{tableName}/data", jsonContent);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    
    public async Task<string> PatchRecordAsync(string tableName, string recordId, object payload, string query = "?columns=id")
    {
        var url = $"{_baseUrl}/tables/{tableName}/data/{recordId}{query}";

        // Serialize the payload to JSON
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Create an HttpRequestMessage for PATCH
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = jsonContent
        };

        // Send the PATCH request
        var response = await _client.SendAsync(request);

        // Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // Return the response content
        return await response.Content.ReadAsStringAsync();
    }
}

public class Meta
{
    public Page Page { get; set; }
}

public class Page
{
    public string Cursor { get; set; }
    public bool More { get; set; }
    public int Size { get; set; }
}

public class Record
{
    public string Id { get; set; }
    public string Name { get; set; }
    [JsonPropertyName("runtime_minutes")]
    public int RuntimeMinutes { get; set; }
    public XataMeta Xata { get; set; }
}

public class XataMeta
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Version { get; set; }
}

public class XataResponse
{
    public Meta Meta { get; set; }
    public List<Record> Records { get; set; }
}
