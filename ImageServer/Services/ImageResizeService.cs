using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using ImageServer.Models;

namespace ImageServer.Services;

public class ImageResizeService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<IResult> ResizeImageAsync(ResizeQueryParamsDto resizeParams)
    {
        try
        {
            ValidateParameters(resizeParams.ImageUrl, resizeParams.Width, resizeParams.Height);

            using var imageResponse = await FetchImageAsync(resizeParams.ImageUrl!);
            var contentType = GetContentType(imageResponse);

            if (contentType == "image/svg+xml")
            {
                return await ReturnSvgImageAsync(imageResponse);
            }

            using var imageStream = await imageResponse.Content.ReadAsStreamAsync();
            using var image = await Image.LoadAsync(imageStream);

            if (NeedsResize(image, resizeParams.Width, resizeParams.Height))
            {
                ResizeImage(image, resizeParams.Width, resizeParams.Height);
            }

            var outputStream = new MemoryStream();
            await SaveImageAsync(image, outputStream, resizeParams.Quality, resizeParams.Format);

            return Results.Stream(outputStream, GetMimeType(resizeParams.Format));
        }
        catch
        {
            return Results.BadRequest("Example: http://<image-server>/?u=<[Required]ImageUrl>&w=<ImageWidth>&h=<ImageHeight>&q=<ImageQuality>&f=<webp(default),jpg,png>");
        }
    }

    private static void ValidateParameters(string? imageUrl, int? width, int? height)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute) || (!width.HasValue && !height.HasValue))
        {
            throw new ArgumentException("Invalid input parameters.");
        }
    }

    private async Task<HttpResponseMessage> FetchImageAsync(string imageUrl)
    {
        var response = await _httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead); // Stream directly
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Failed to fetch the image.");
        }
        return response;
    }

    private static string? GetContentType(HttpResponseMessage imageResponse)
    {
        var contentType = imageResponse.Content.Headers.ContentType?.MediaType;
        if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("image/"))
        {
            throw new InvalidOperationException("Invalid image file.");
        }
        return contentType;
    }

    private static async Task<IResult> ReturnSvgImageAsync(HttpResponseMessage imageResponse)
    {
        var svgStream = await imageResponse.Content.ReadAsStreamAsync();
        return Results.Stream(svgStream, "image/svg+xml");
    }

    private static bool NeedsResize(Image image, int? width, int? height)
    {
        return (width.HasValue && width.Value != image.Width) || (height.HasValue && height.Value != image.Height);
    }

    private static void ResizeImage(Image image, int? width, int? height)
    {
        var currentWidth = image.Width;
        var currentHeight = image.Height;

        if ((width.HasValue && currentWidth >= width.Value) || (height.HasValue && currentHeight >= height.Value))
        {
            image.Mutate(x => x.Resize(width ?? currentWidth, height ?? currentHeight));
        }
    }

    private static async Task SaveImageAsync(Image image, Stream outputStream, int? quality, string? format)
    {
        var imageQuality = Math.Clamp(quality ?? 75, 1, 100);

        switch (format?.ToLower())
        {
            case "jpeg":
            case "jpg":
                await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = imageQuality });
                break;
            case "png":
                await image.SaveAsPngAsync(outputStream);
                break;
            default:
                await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = imageQuality, SkipMetadata = true });
                break;
        }

        outputStream.Position = 0;
    }

    private static string GetMimeType(string? format)
    {
        return format?.ToLower() switch
        {
            "jpeg" or "jpg" => "image/jpeg",
            "png" => "image/png",
            _ => "image/webp"
        };
    }
}
