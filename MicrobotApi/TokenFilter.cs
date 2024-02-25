﻿using Microsoft.AspNetCore.SignalR;

namespace MicrobotApi;

public class TokenFilter : IHubFilter
{
    private readonly ConnectionManager _connectionManager;
    private const string invalidTokenMessage = "Invalid token!";
    
    public TokenFilter(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }
    public async ValueTask<object> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
    {

        var token = GetToken(invocationContext.Context);

        var exists = _connectionManager.Connections.Contains(token);
        if (!exists)
            throw new UnauthorizedAccessException(invalidTokenMessage);
        
        Console.WriteLine($"Calling hub method '{invocationContext.HubMethodName}'");
        try
        {
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception calling '{invocationContext.HubMethodName}': {ex}");
            throw;
        }
    }

    // Optional method
    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        return next(context);
    }

    // Optional method
    public Task OnDisconnectedAsync(
        HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
    {
        return next(context, exception);
    }

    private string? GetToken(HubCallerContext context)
    {
        var httpContext = context.GetHttpContext();

        var validHttpRequest = httpContext is { Request.Headers: not null } && httpContext.Request.Headers.Any();
        
        if (!validHttpRequest 
            || (string.IsNullOrWhiteSpace(httpContext?.Request.Headers?["token"]) 
                && string.IsNullOrWhiteSpace(httpContext?.Request.Query["token"].FirstOrDefault())))
        {
            return null;
        }

        var token = httpContext.Request.Headers?["token"].FirstOrDefault() 
                          ?? httpContext.Request.Query["token"].FirstOrDefault();

        return token;
    }
}