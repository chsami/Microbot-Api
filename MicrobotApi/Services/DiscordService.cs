using System.Text;
using System.Text.Json;
using MicrobotApi.Models.Discord;

namespace MicrobotApi.Services;

public class DiscordService(HttpClient httpClient)
{
    
    public async Task<string> RefreshAccessToken(string? clientId, string? clientSecret, string refreshToken, string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new Exception("Could not refresh token because clientid is empty");
        if ( string.IsNullOrWhiteSpace(clientSecret))
            throw new Exception("Could not refresh token because clientsecret is empty");
        
        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });

        var response = await httpClient.PostAsync("https://discord.com/api/oauth2/token", requestContent);
        var jsonResponse = await response.Content.ReadFromJsonAsync<OAuthResponse>();

        if (response.IsSuccessStatusCode)
        {
            return jsonResponse.Access_Token;
        }

        // Handle error response
        throw new Exception("Could not refresh token: " + jsonResponse);
    }


    public async Task<TokenResponse?> GetToken(string clientId, string clientSecret, string code, string redirectUri)
    {
        var values = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri }
        };

        var content = new FormUrlEncodedContent(values);

        var response = await httpClient.PostAsync("oauth2/token", content);

        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            
            if (token?.Access_Token == null) return null;

            return token;
        }
        return null;
    }

    public async Task<DiscordUser?> GetUserInfo(string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("users/@me");
        var userInfo = await response.Content.ReadFromJsonAsync<DiscordUser>();
        return userInfo;
    }
}
