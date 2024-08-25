using ImageServer.Models;
using ImageServer.Services;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddCors();
builder.Services.AddProblemDetails();
builder.Services.AddOutputCache(options =>
{
    options.UseCaseSensitivePaths = false;
    options.DefaultExpirationTimeSpan = TimeSpan.FromDays(1);
    options.AddBasePolicy(policy => policy.SetVaryByQuery("*"));
});

builder.Services.AddHttpClient<ImageResizeService>();
builder.Services.AddScoped<ImageResizeService>();

var app = builder.Build();
app.UseOutputCache();
app.UseCors(options =>
{
    options.AllowAnyHeader()
           .AllowAnyMethod()
           .AllowAnyOrigin();
});

app.MapGet("/resize", async ([AsParameters] ResizeQueryParamsDto queryParams, ImageResizeService resizeService) => 
    await resizeService.ResizeImageAsync(queryParams))
    .CacheOutput();

await app.RunAsync();
