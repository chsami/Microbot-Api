using MicrobotApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder
            .WithOrigins(
                    "http://localhost:4200", "https://*.vercel.app")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddSignalR()
    .AddHubOptions<MicrobotHub>(options =>
    {
        // Local filters will run second
        options.AddFilter<TokenFilter>();
    });

builder.Services.AddSingleton<ConnectionManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/token", () =>
    {
        var token = Guid.NewGuid();
        var connectionManager = app.Services.GetService<ConnectionManager>();
        connectionManager?.Connections.Add(token.ToString());
        return token;
    })
    .WithName("getToken")
    .WithOpenApi();
app.MapPost("/token", async ([FromBody] TokenRequestModel tokenRequestModel) =>
    {
        var connectionManager = app.Services.GetService<ConnectionManager>();
        return connectionManager?.Connections.Contains(tokenRequestModel.Token);
    })
    .WithName("Check Token Validity")
    .WithOpenApi();
app.UseCors();
app.MapHub<MicrobotHub>("/microbot");

app.Run();