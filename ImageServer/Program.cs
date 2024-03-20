using ImageServer.Contexts;
using ImageServer.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, CustomJsonSerializerContext.Default);
});

builder.Services.AddHttpClient<ImageResizeService>();
builder.Services.AddScoped<ImageResizeService>();
var app = builder.Build();
app.MapGet("/resize", async (
    [FromQuery(Name = "u")] string? imageUrl,
    [FromQuery(Name = "w")] int? width,
    [FromQuery(Name = "w")] int? height,
    [FromQuery(Name = "w")] int? quality,
    ImageResizeService resizeService) =>
{
    return await resizeService.ResizeImageAsync(imageUrl, width, height, quality);
});

app.Run();
