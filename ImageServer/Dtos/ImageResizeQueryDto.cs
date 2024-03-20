using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace ImageServer.Dtos;

public record ImageResizeQueryDto
{
    [FromQuery]
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [FromQuery]
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [FromQuery]
    [JsonPropertyName("height")]
    public int Height { get; set; }

    [FromQuery]
    [JsonPropertyName("quality")]
    public int Quality { get; set; } = 75;
}


[JsonSerializable(typeof(ImageResizeQueryDto))]
[JsonSerializable(typeof(ProblemDetails))]
public partial class ImageResizeQueryDtoSerializer : JsonSerializerContext
{
}
