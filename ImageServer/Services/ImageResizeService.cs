using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace ImageServer.Services;

public class ImageResizeService(HttpClient httpClient)
{
    public async Task<IResult> ResizeImageAsync(string? imageUrl, int? w, int? h, int? q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || !Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                throw new Exception("Invalid image URL.");

            if (!w.HasValue && !h.HasValue)
                throw new Exception("Invalid dimensions for resizing.");

            if (q.HasValue && (q <= 0 || q > 100))
                throw new Exception("Invalid quality value.");

            int width = w ?? 0;
            int height = h ?? 0;
            int quality = q ?? 75;

            var response = await httpClient.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch image.");

            using var image = await Image.LoadAsync(await response.Content.ReadAsStreamAsync());
            image.Mutate(x => x.Resize(width, height));

            var outputStream = new MemoryStream();
            var encoder = new WebpEncoder { Quality = Math.Clamp(quality, 1, 100), SkipMetadata = true };
            await image.SaveAsWebpAsync(outputStream, encoder);
            outputStream.Position = 0;

            return Results.Stream(outputStream, "image/webp");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
