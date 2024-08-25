using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ImageServer.Services;

public class ImageResizeService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<IResult> ResizeImageAsync(string? imageUrl, int? width, int? height, int? quality, string? format)
    {
        try
        {
            ValidateParameters(imageUrl, width, height);

            using var imageResponse = await FetchImageAsync(imageUrl!);
            var contentType = GetContentType(imageResponse);

            if (IsSvg(contentType))
            {
                return await ReturnSvgImageAsync(imageResponse);
            }

            using var image = await LoadImageAsync(imageResponse);
            ResizeImage(image, width, height);

            var outputStream = new MemoryStream();
            await SaveImageAsync(image, outputStream, quality, format);

            return Results.Stream(outputStream, GetMimeType(format));
        }
        catch
        {
            return Results.BadRequest("Example: http://<image-server>/?u=<[Required]ImageUrl>&w=<ImageWidth>&h=<ImageHeight>&q=<ImageQuality>&f=<jpg,png,webp>");
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
        var response = await _httpClient.GetAsync(imageUrl);
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

    private static bool IsSvg(string? contentType)
    {
        return contentType == "image/svg+xml";
    }

    private static async Task<IResult> ReturnSvgImageAsync(HttpResponseMessage imageResponse)
    {
        var svgStream = await imageResponse.Content.ReadAsStreamAsync();
        return Results.Stream(svgStream, "image/svg+xml");
    }

    private static async Task<Image> LoadImageAsync(HttpResponseMessage imageResponse)
    {
        using var imageStream = await imageResponse.Content.ReadAsStreamAsync();
        return await Image.LoadAsync(imageStream);
    }

    private static void ResizeImage(Image image, int? width, int? height)
    {
        image.Mutate(x => x.Resize(width ?? 0, height ?? 0));
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
