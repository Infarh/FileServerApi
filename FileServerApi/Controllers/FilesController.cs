using System.Net.Mime;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;

namespace FileServerApi.Controllers;

[ApiController, Route("api/v1/files")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)] 
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _Environment;
    private readonly IConfiguration _Configuration;
    private readonly ILogger<FilesController> _Logger;

    public FilesController(IWebHostEnvironment Environment, IConfiguration Configuration, ILogger<FilesController> Logger)
    {
        _Environment = Environment;
        _Configuration = Configuration;
        _Logger = Logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetAllFilesNames()
    {
        _Logger.LogInformation("User {0}", User.Identity?.Name ?? "--null--");

        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration["ContentDir"]);
        if (dir.Any())
            return Ok(dir.Select(f => f.Name));
        return NoContent();
    }

    [HttpGet("{FileName}/content")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, MediaTypeNames.Application.Octet)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetFileContent(string FileName)
    {
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) 
            return NotFound(new { FileName });

        return File(file.CreateReadStream(), 
            contentType: new FileExtensionContentTypeProvider().TryGetContentType(file.Name, out var content_type) 
                ? content_type 
                : MediaTypeNames.Application.Octet, 
            fileDownloadName: file.Name, 
            lastModified: file.LastModified, 
            entityTag: EntityTagHeaderValue.Any,
            enableRangeProcessing: true);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var files_path = _Configuration.GetValue("ContentDir", "/");
        //var file_name = FileName;
        var file_name = file.FileName;

        var dest_path = _Environment.ContentRootFileProvider.GetFileInfo(Path.Combine(files_path, file_name));
        var destination_file = new FileInfo(dest_path.PhysicalPath);
        await using var destination = destination_file.Create();
        await file.CopyToAsync(destination);

        destination_file.Refresh();
        return Ok(new { FileName = file_name, destination_file.Length });
    }

    [HttpDelete("{FileName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteFile(string FileName)
    {
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file)
            return NotFound(new { FileName });

        var physical_file = new FileInfo(file.PhysicalPath);
        physical_file.Delete();

        _Logger.LogInformation("Файл {0} удалён", physical_file);

        return Ok(new
        {
            FileName,
            physical_file.Length,
        });
    }

    [HttpPost("{FileSourceName}/copyto/{FileDestinationName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CopyFile(string FileSourceName, string FileDestinationName)
    {
        var files_path = _Configuration.GetValue("ContentDir", "/");
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(files_path);
        var dest_path = _Environment.ContentRootFileProvider.GetFileInfo(Path.Combine(files_path, FileDestinationName));

        if (dir.FirstOrDefault(f => f.Name == FileSourceName) is not { Exists: true } file) 
            return NotFound(new { FileSourceName });

        var source_file = new FileInfo(file.PhysicalPath);
        var destination_file = new FileInfo(dest_path.PhysicalPath);

        source_file.CopyTo(destination_file.FullName, true);

        _Logger.LogInformation("Файл {0} удалён в {1}", source_file, destination_file);

        return Ok(new
        {
            Source = FileSourceName,
            Destination = FileDestinationName
        });
    }

    private static async Task<string> HashStreamAsync(HashAlgorithm hasher, Stream file_stream)
    {
        var hash = await hasher.ComputeHashAsync(file_stream);
        var hash_str = string.Join("", hash.Select(b => b.ToString("X2")));
        return hash_str;
    }

    private async Task<IActionResult> HashFileAsync(FileInfo server_file, HashAlgorithm hasher)
    {
        await using var file_stream = server_file.OpenRead();
        var hash_str = await HashStreamAsync(hasher, file_stream);
        return Content(hash_str);
    }

    [HttpGet("{FileName}/MD5")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMD5(string FileName)
    {
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) 
            return NotFound(new { FileName });

        var server_file = new FileInfo(file.PhysicalPath);

        using var hasher = System.Security.Cryptography.MD5.Create();
        return await HashFileAsync(server_file, hasher);
    }

    [HttpGet("{FileName}/SHA1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSHA1(string FileName)
    {
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) 
            return NotFound(new { FileName });

        var server_file = new FileInfo(file.PhysicalPath);

        using var hasher = SHA1.Create();
        return await HashFileAsync(server_file, hasher);
    }

    [HttpGet("{FileName}/SHA256")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSHA256(string FileName)
    {
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) 
            return NotFound(new { FileName });

        var server_file = new FileInfo(file.PhysicalPath);

        using var hasher = SHA256.Create();
        return await HashFileAsync(server_file, hasher);
    }

    [HttpGet("{FileName}/SHA384")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSHA384(string FileName)
    {
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) 
            return NotFound(new { FileName });

        var server_file = new FileInfo(file.PhysicalPath);

        using var hasher = SHA384.Create();
        return await HashFileAsync(server_file, hasher);
    }

    [HttpGet("{FileName}/SHA512")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSHA512(string FileName)
    {
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) 
            return NotFound(new { FileName });

        var server_file = new FileInfo(file.PhysicalPath);

        using var hasher = SHA512.Create();
        return await HashFileAsync(server_file, hasher);
    }

    [HttpPost("MD5")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMD5(IFormFile file)
    {
        using var hasher = MD5.Create();
        var hash = await HashStreamAsync(hasher, file.OpenReadStream());
        return Content(hash);
    }

    [HttpPost("SHA1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSHA1(IFormFile file)
    {
        using var hasher = SHA1.Create();
        var hash = await HashStreamAsync(hasher, file.OpenReadStream());
        return Content(hash);
    }

    [HttpPost("SHA256")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSHA256(IFormFile file)
    {
        using var hasher = SHA256.Create();
        var hash = await HashStreamAsync(hasher, file.OpenReadStream());
        return Content(hash);
    }

    [HttpPost("SHA384")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSHA384(IFormFile file)
    {
        using var hasher = SHA384.Create();
        var hash = await HashStreamAsync(hasher, file.OpenReadStream());
        return Content(hash);
    }

    [HttpPost("SHA512")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSHA512(IFormFile file)
    {
        using var hasher = SHA512.Create();
        var hash = await HashStreamAsync(hasher, file.OpenReadStream());
        return Content(hash);
    }
}
