using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using ImageServer.Dtos;
using SixLabors.ImageSharp.Formats.Jpeg;

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
            var encoder = new JpegEncoder
            {
                Quality = queryParams.Quality
            };
            await image.SaveAsJpegAsync(outputStream, encoder);
            outputStream.Position = 0;

            return Results.Stream(outputStream, "image/jpeg");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
