using Microsoft.AspNetCore.Mvc;

namespace ImageServer.Models;

public class ResizeQueryParamsDto
{
    [FromQuery(Name = "u")]
    public string? ImageUrl { get; set; }

    [FromQuery(Name = "w")]
    public int? Width { get; set; }

    [FromQuery(Name = "h")]
    public int? Height { get; set; }

    [FromQuery(Name = "q")]
    public int? Quality { get; set; }

    [FromQuery(Name = "f")]
    public string? Format { get; set; }
}