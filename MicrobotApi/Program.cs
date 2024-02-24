using MicrobotApi;

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
builder.Services.AddSignalR();

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
        return token;
    })
    .WithName("getToken")
    .WithOpenApi();
app.UseCors();
app.MapHub<ChatHub>("/microbot");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}