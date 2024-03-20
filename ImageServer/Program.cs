
using ImageServer.Dtos;
using ImageServer.Services;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ImageResizeQueryDtoSerializer.Default);
});

builder.Services.AddHttpClient<ImageResizeService>();
builder.Services.AddScoped<ImageResizeService>();

var app = builder.Build();

app.MapGet("/resize", async ([AsParameters] ImageResizeQueryDto queryParams, ImageResizeService resizeService) =>
{
    return await resizeService.ResizeImageAsync(queryParams);
});


app.Run();
