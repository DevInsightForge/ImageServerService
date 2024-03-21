using ImageServer.Contexts;
using ImageServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(options =>
    options.DefaultExpirationTimeSpan = TimeSpan.FromDays(1));
builder.Services.ConfigureHttpJsonOptions(options => 
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, CustomJsonSerializerContext.Default));

builder.Services.AddHttpClient<ImageResizeService>();
builder.Services.AddScoped<ImageResizeService>();

var app = builder.Build();

app.UseOutputCache();
app.UseResponseCaching();

app.Use(async (context, next) =>
{
    context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
    {
        Public = true,
        MaxAge = TimeSpan.FromDays(1),
    };
    await next();
});

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
