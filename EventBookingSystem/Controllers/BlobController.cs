using Microsoft.AspNetCore.Mvc;

public class BlobController : Controller
{
    private readonly BlobStorageService _blobService;

    public BlobController(BlobStorageService blobService)
    {
        _blobService = blobService;
    }

    public async Task<IActionResult> Index()
    {
        var imageUrls = await _blobService.ListImageUrlsAsync("images2");
        return View(imageUrls); // Pass to view as model
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            await _blobService.UploadFileAsync("images2", file.FileName, stream);
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Download(string fileName)
    {
        var stream = await _blobService.DownloadFileAsync("images2", fileName);
        return File(stream, "application/octet-stream", fileName);
    }

    public async Task<IActionResult> Delete(string fileName)
    {
        await _blobService.DeleteFileAsync("mycontainer", fileName);
        return RedirectToAction("Index");
    }

    
}
