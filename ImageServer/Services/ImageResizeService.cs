using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using ImageServer.Dtos;

namespace ImageServer.Services;

public class ImageResizeService(HttpClient httpClient)
{
    public async Task<IResult> ResizeImageAsync(ImageResizeQueryDto queryParams)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryParams.ImageUrl) || queryParams.Width <= 0 || queryParams.Height <= 0)
            {
                return Results.BadRequest("Invalid parameters.");
            }

            using var response = await httpClient.GetAsync(queryParams.ImageUrl);
            if (!response.IsSuccessStatusCode)
            {
                return Results.Problem("Failed to fetch image from URL.");
            }

            using var image = await Image.LoadAsync(response.Content.ReadAsStream());
            image.Mutate(x => x.Resize(queryParams.Width, queryParams.Height));

            var outputStream = new MemoryStream();
            var encoder = new WebpEncoder
            {
                Quality = queryParams.Quality
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
