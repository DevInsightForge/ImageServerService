using ImageServer.Contexts;
using ImageServer.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddResponseCaching();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, CustomJsonSerializerContext.Default);
});
builder.Services.AddOutputCache();
builder.Services.AddResponseCaching();

builder.Services.AddHttpClient<ImageResizeService>();
builder.Services.AddScoped<ImageResizeService>();

var app = builder.Build();
app.UseOutputCache();
app.UseResponseCaching();

app.MapGet("/resize", async (
    [FromQuery(Name = "u")] string? imageUrl,
    [FromQuery(Name = "w")] int? width,
    [FromQuery(Name = "h")] int? height,
    [FromQuery(Name = "q")] int? quality,
    ImageResizeService resizeService) =>
{
    return await resizeService.ResizeImageAsync(imageUrl, width, height, quality);
}).CacheOutput();

app.Run();
