namespace MicrobotApi.Models;

public class SendBotPluginRequestModel : IHubRequestModel
{
    public string Group { get; set; }
    public List<PluginRequestModel> Plugins { get; set; }
}