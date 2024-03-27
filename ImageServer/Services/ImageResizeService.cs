using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Net;

namespace ImageServer.Services;

public class ImageResizeService(HttpClient httpClient)
{
    public async Task<IResult> ResizeImageAsync(string? imageUrl, int? w, int? h, int? q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || !Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute) || (!w.HasValue && !h.HasValue))
                throw new ArgumentException("Invalid input parameters.");

            using var imageResponse = await httpClient.GetAsync(imageUrl);

            if (!imageResponse.IsSuccessStatusCode)
                throw new Exception("Failed to fetch the image.");

            var contentType = imageResponse.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("image/"))
                throw new InvalidOperationException("The URL does not point to a valid image file.");

            using var imageStream = await imageResponse.Content.ReadAsStreamAsync();
            using var image = await Image.LoadAsync(imageStream);
            image.Mutate(x => x.Resize(w ?? 0, h ?? 0));

            var outputStream = new MemoryStream();
            await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = Math.Clamp(q ?? 75, 1, 100), SkipMetadata = true });
            outputStream.Position = 0;

            return Results.Stream(outputStream, "image/webp");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex}");
            return Results.Problem(
                detail: "Example format: http://<image-server>/?u=<[Required]ImageUrl>&w=<ImageWidth>&h=<ImageHeight>&q=<ImageQuality>",
                statusCode: (int)HttpStatusCode.BadRequest);
        }
    }
}
