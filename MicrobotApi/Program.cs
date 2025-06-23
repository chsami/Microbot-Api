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
                    "https://themicrobot.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

StripeConfiguration.ApiKey = builder.Configuration["Stripe:ApiKey"];

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
builder.Services.AddScoped<AzureStorageService>();
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