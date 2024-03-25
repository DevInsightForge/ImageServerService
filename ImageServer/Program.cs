using ImageServer.Contexts;
using ImageServer.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOutputCache(options =>
{
    options.UseCaseSensitivePaths = false;
    options.DefaultExpirationTimeSpan = TimeSpan.FromDays(1);
    options.AddBasePolicy(policy => policy.SetVaryByQuery("*"));
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, CustomJsonSerializerContext.Default);
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

app.MapGet("/", async (
    [FromQuery(Name = "u")] string? imageUrl,
    [FromQuery(Name = "w")] int? width,
    [FromQuery(Name = "h")] int? height,
    [FromQuery(Name = "q")] int? quality,
    ImageResizeService resizeService) =>
{
    return await resizeService.ResizeImageAsync(imageUrl, width, height, quality);
}).CacheOutput(options => options.SetVaryByQuery("*"));

app.Run();
