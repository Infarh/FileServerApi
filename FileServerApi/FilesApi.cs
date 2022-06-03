using System.Security.Cryptography;

using Microsoft.AspNetCore.StaticFiles;

namespace FileServerApi;

internal static class FilesApi
{
    public static IEndpointRouteBuilder UseFilesApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/files", GetAllFiles)
           .WithName("Files")
           .Produces<string[]>();

        app.MapGet("/files/{FileName}", GetFileContent)
           .WithName("FileContent");

        app.MapGet("/files/{FileName}/info", GetFileInfo)
           .WithName("FileInfo")
           .Produces(200)
           .Produces(404);

        app.MapPost("/files/{FileName}", UploadFile).WithName("FileUpload")
           .Accepts<IFormFile>("multipart/form-data")
           .Produces(200);

        app.MapDelete("/files/{FileName}", DeleteFile).WithName("DeleteFile").Produces(200).Produces(404);

        app.MapGet("/files/{FileName}/md5", GetMD5).WithName("MD5").Produces<string>().Produces(404);
        app.MapGet("/files/{FileName}/SHA1", GetSHA1).WithName("SHA1").Produces<string>().Produces(404);
        app.MapGet("/files/{FileName}/SHA256", GetSHA256).WithName("SHA256").Produces<string>().Produces(404);
        app.MapGet("/files/{FileName}/SHA384", GetSHA384).WithName("SHA384").Produces<string>().Produces(404);
        app.MapGet("/files/{FileName}/SHA512", GetSHA512).WithName("SHA512").Produces<string>().Produces(404);

        app.MapPost("/files/{FileSourceName}/copyto/{FileDestinationName}", CopyFile).WithName("CopyFile").Produces(200).Produces(404);

        return app;
    }

    private static async Task<IResult> CopyFile(IWebHostEnvironment env, IConfiguration cfg, string FileSourceName, string FileDestinationName)
    {
        var files_path = cfg.GetValue("ContentDir", "/");
        var dir = env.ContentRootFileProvider.GetDirectoryContents(files_path);
        var dest_path = env.ContentRootFileProvider.GetFileInfo(Path.Combine(files_path, FileDestinationName));

        if (dir.FirstOrDefault(f => f.Name == FileSourceName) is not { Exists: true } file) return Results.NotFound(new { FileSourceName });
        var source_file = new FileInfo(file.PhysicalPath);
        var destination_file = new FileInfo(dest_path.PhysicalPath);

        source_file.CopyTo(destination_file.FullName, true);

        return Results.Ok(new
        {
            Source = FileSourceName,
            Destination = FileDestinationName
        });
    }

    private static async Task<IResult> GetMD5(IWebHostEnvironment env, IConfiguration cfg, string FileName)
    {
        var dir = env.ContentRootFileProvider.GetDirectoryContents(cfg.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) return Results.NotFound(new { FileName });
        var server_file = new FileInfo(file.PhysicalPath);

        await using var file_stream = server_file.OpenRead();
        using var hasher = MD5.Create();
        var hash = await hasher.ComputeHashAsync(file_stream);

        var hash_str = string.Join("", hash.Select(b => b.ToString("X2")));

        return Results.Text(hash_str);
    }

    private static async Task<IResult> GetSHA1(IWebHostEnvironment env, IConfiguration cfg, string FileName)
    {
        var dir = env.ContentRootFileProvider.GetDirectoryContents(cfg.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) return Results.NotFound(new { FileName });
        var server_file = new FileInfo(file.PhysicalPath);

        await using var file_stream = server_file.OpenRead();
        using var hasher = SHA1.Create();
        var hash = await hasher.ComputeHashAsync(file_stream);

        var hash_str = string.Join("", hash.Select(b => b.ToString("X2")));

        return Results.Text(hash_str);
    }

    private static async Task<IResult> GetSHA256(IWebHostEnvironment env, IConfiguration cfg, string FileName)
    {
        var dir = env.ContentRootFileProvider.GetDirectoryContents(cfg.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) return Results.NotFound(new { FileName });
        var server_file = new FileInfo(file.PhysicalPath);

        await using var file_stream = server_file.OpenRead();
        using var hasher = SHA256.Create();
        var hash = await hasher.ComputeHashAsync(file_stream);

        var hash_str = string.Join("", hash.Select(b => b.ToString("X2")));

        return Results.Text(hash_str);
    }

    private static async Task<IResult> GetSHA384(IWebHostEnvironment env, IConfiguration cfg, string FileName)
    {
        var dir = env.ContentRootFileProvider.GetDirectoryContents(cfg.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) return Results.NotFound(new { FileName });
        var server_file = new FileInfo(file.PhysicalPath);

        await using var file_stream = server_file.OpenRead();
        using var hasher = SHA384.Create();
        var hash = await hasher.ComputeHashAsync(file_stream);

        var hash_str = string.Join("", hash.Select(b => b.ToString("X2")));

        return Results.Text(hash_str);
    }

    private static async Task<IResult> GetSHA512(IWebHostEnvironment env, IConfiguration cfg, string FileName)
    {
        var dir = env.ContentRootFileProvider.GetDirectoryContents(cfg.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) return Results.NotFound(new { FileName });
        var server_file = new FileInfo(file.PhysicalPath);

        await using var file_stream = server_file.OpenRead();
        using var hasher = SHA512.Create();
        var hash = await hasher.ComputeHashAsync(file_stream);

        var hash_str = string.Join("", hash.Select(b => b.ToString("X2")));

        return Results.Text(hash_str);
    }

    private static IResult GetFileInfo(IWebHostEnvironment env, IConfiguration cfg, string FileName)
    {
        var dir = env.ContentRootFileProvider.GetDirectoryContents(cfg.GetValue("ContentDir", "/"));
        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) 
            return Results.NotFound(new { FileName });

        return Results.Json(new
        {
            FileName = file.Name,
            file.Length, 
            Ext = Path.GetExtension(file.Name),
            file.LastModified,
        });
    }

    private static IResult DeleteFile(IWebHostEnvironment env, IConfiguration cfg, string FileName)
    {
        var dir = env.ContentRootFileProvider.GetDirectoryContents(cfg.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) 
            return Results.NotFound(new { FileName });

        File.Delete(file.PhysicalPath);

        return Results.Ok(new { FileName });
    }

    private static async Task<IResult> UploadFile(IWebHostEnvironment env, IConfiguration cfg, string FileName, HttpRequest request)
    {
        var files_path = cfg.GetValue("ContentDir", "/");
        var file_name = FileName;

        var dest_path = env.ContentRootFileProvider.GetFileInfo(Path.Combine(files_path, file_name));
        var destination_file = new FileInfo(dest_path.PhysicalPath);
        await using var destination = destination_file.Create();
        await request.BodyReader.CopyToAsync(destination);

        destination_file.Refresh();
        return Results.Ok(new { FileName, destination_file.Length });
    }

    private static IResult GetFileContent(IWebHostEnvironment env, IConfiguration cfg, string FileName)
    {
        var dir = env.ContentRootFileProvider.GetDirectoryContents(cfg.GetValue("ContentDir", "/"));

        if (dir.FirstOrDefault(f => f.Name == FileName) is not { Exists: true } file) return Results.NotFound(new { FileName });

        var content_type_provider = new FileExtensionContentTypeProvider();
        if (content_type_provider.TryGetContentType(file.Name, out var content_type)) 
            return Results.File(file.CreateReadStream(), fileDownloadName: file.Name, enableRangeProcessing: true, lastModified: file.LastModified, contentType: content_type);

        return Results.File(file.CreateReadStream(), fileDownloadName: file.Name, enableRangeProcessing: true, lastModified: file.LastModified);
    }

    private static IResult GetAllFiles(IWebHostEnvironment env, IConfiguration cfg)
    {
        var dir = env.ContentRootFileProvider.GetDirectoryContents(cfg["ContentDir"]);
        return Results.Ok(dir.Select(f => f.Name));
    }
}
