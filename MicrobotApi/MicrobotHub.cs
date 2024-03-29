﻿using MicrobotApi.Models;
using Microsoft.AspNetCore.SignalR;

namespace MicrobotApi;

public class MicrobotHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user,message);
    }
    
    public async Task AddToGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
    }

    public async Task RemoveFromGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
    }
    
    public async Task SendBotPlugins(SendBotPluginRequestModel sendBotPluginRequestModel)
    {
        await Clients.Group(sendBotPluginRequestModel.Group)
            .SendAsync("ReceiveBotPlugins", sendBotPluginRequestModel.Plugins);
    }
}