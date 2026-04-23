using System.Text.Json;
using App.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

App.Persistence.DependencyInjectionConfig.Inject(builder.Services);
App.Application.DependencyInjectionConfig.Inject(builder.Services);

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5175", "https://localhost:7280")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

builder.Services.AddControllers()
    .AddJsonOptions(x => 
        x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

app.UseHttpsRedirection();

app.UseCors("WebClient");

app.MapControllers();

app.Run();