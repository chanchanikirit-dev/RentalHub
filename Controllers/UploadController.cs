using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Storage;
using System.Text;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly Supabase.Client _supabase;
    private readonly string _bucket;

    public UploadController(IConfiguration configuration)
    {
        var url = configuration["Supabase:BaseUrl"];
        var key = configuration["Supabase:ServiceKey"];
        _bucket = configuration["Supabase:Bucket"];

        _supabase = new Supabase.Client(url, key);
    }

    [HttpPost("item-image")]
    public async Task<IActionResult> UploadItemImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            var fileExt = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExt}";

            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            var storage = _supabase.Storage
                .From(_bucket);

            var response = await storage.Upload(
                fileBytes,
                fileName,
                new Supabase.Storage.FileOptions
                {
                    ContentType = file.ContentType,
                    Upsert = false
                }
            );

            if (response == null)
                return StatusCode(500, "Upload failed");

            var publicUrl = storage.GetPublicUrl(fileName);

            return Ok(new
            {
                photoUrl = publicUrl
            });
        }
        catch (Exception ex)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            var fileExt = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExt}";

            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            var storage = _supabase.Storage
                .From(_bucket);

            var response = await storage.Upload(
                fileBytes,
                fileName,
                new Supabase.Storage.FileOptions
                {
                    ContentType = file.ContentType,
                    Upsert = false
                }
            );

            if (response == null)
                return StatusCode(500, "Upload failed");

            var publicUrl = storage.GetPublicUrl(fileName);

            return Ok(new
            {
                photoUrl = publicUrl
            });
        }
    }
}
