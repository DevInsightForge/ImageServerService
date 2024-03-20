using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace ImageServer.Dtos;

public record ImageResizeQueryDto
{
    public string ImageUrl { get; set; } = string.Empty;

    public int Width { get; set; }

    public int Height { get; set; }

    public int Quality { get; set; } = 75;
}


[JsonSerializable(typeof(ImageResizeQueryDto))]
[JsonSerializable(typeof(ProblemDetails))]
public partial class ImageResizeQueryDtoSerializer : JsonSerializerContext
{
}
