using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Net.Http.Headers;

namespace FileServerApi.Controllers;

[ApiController, Route("api/v1/files")]
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
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration["ContentDir"]);
        if (dir.Any())
            return Ok(dir.Select(f => f.Name));
        return NoContent();
    }

    [HttpGet("{FileName}/content")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteFile(string FileName)
    {
        var dir = _Environment.ContentRootFileProvider.GetDirectoryContents(_Configuration.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file)
            return NotFound(new { FileName });

        var physical_file = new FileInfo(file.PhysicalPath);
        physical_file.Delete();

        return Ok(new
        {
            FileName,
            physical_file.Length,
        });
    }

    [HttpPost("{FileSourceName}/copyto/{FileDestinationName}")]
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

        return Ok(new
        {
            Source = FileSourceName,
            Destination = FileDestinationName
        });
    }

    private async Task<IActionResult> HashFile(FileInfo server_file, HashAlgorithm hasher)
    {
        await using var file_stream = server_file.OpenRead();
        var hash = await hasher.ComputeHashAsync(file_stream);

        var hash_str = string.Join("", hash.Select(b => b.ToString("X2")));

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
        return await HashFile(server_file, hasher);
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
        return await HashFile(server_file, hasher);
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
        return await HashFile(server_file, hasher);
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
        return await HashFile(server_file, hasher);
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
        return await HashFile(server_file, hasher);
    }
}
