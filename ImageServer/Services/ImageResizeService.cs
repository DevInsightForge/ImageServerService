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
            var width = w ?? 0;
            var height = h ?? 0;
            var quality = q ?? 0;

            if (0 >= quality || quality > 100) quality = 75;

            if (string.IsNullOrWhiteSpace(imageUrl) || (width <= 0 && height <= 0))
            {
                return Results.BadRequest("Invalid parameters.");
            }

            using var response = await httpClient.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
            {
                return Results.Problem("Failed to fetch image from URL.");
            }

            using var image = await Image.LoadAsync(response.Content.ReadAsStream());
            image.Mutate(x => x.Resize(width, height));

            var outputStream = new MemoryStream();
            var encoder = new WebpEncoder
            {
                Quality = quality,
                SkipMetadata = true
            };

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
