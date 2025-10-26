// See https://aka.ms/new-console-template for more information
using ChatBackend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Database Ayarı ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Controller ve HttpClient ---
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// --- CORS Ayarı ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// --- Middleware Sırası ---
app.UseCors();
app.MapControllers();

app.Run();
