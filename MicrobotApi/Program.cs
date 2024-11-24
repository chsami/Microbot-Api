using Azure.Storage.Blobs;
using Discord.Commands;
using Discord.WebSocket;
using MicrobotApi;
using MicrobotApi.Database;
using MicrobotApi.Handlers;
using MicrobotApi.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

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

StripeConfiguration.ApiKey = "sk_test_51PHBkLJ45WMcMRTutHNGpJZvzToGIf0EazZTuT38eTwoRbHuAoqajpCUZ1bdmWperK6jazc7wLdHHdX7x0PFdo6R00vEff23me";

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Microbot Api", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddAuthentication(options =>
    {
        // Set the default authentication scheme
        options.DefaultAuthenticateScheme = "DiscordScheme";
        options.DefaultChallengeScheme = "DiscordScheme";
    })
    .AddScheme<CustomAuthenticationOptions, DiscordAuthenticationHandler>("DiscordScheme", options => {});


builder.Services
    .AddSignalR()
    .AddHubOptions<MicrobotHub>(options =>
    {
        // Local filters will run second
        options.AddFilter<TokenFilter>();
    });
builder.Services.AddHttpClient<DiscordService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Discord:Api"]);
});
/*builder.Services.AddSingleton<DiscordBotService>();
builder.Services.AddHostedService<BotHostedService>();*/
builder.Services.AddScoped<AzureStorageService>();
/*builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<CommandService>();*/
builder.Services.AddScoped<XataService>();
builder.Services.AddSingleton(c =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobConnection"))
);
builder.Services.AddDbContext<MicrobotContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MicrobotContext")));
builder.Services.AddMemoryCache();
builder.Services.AddControllers();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();

app.MapHub<MicrobotHub>("/microbot");

app.Run();