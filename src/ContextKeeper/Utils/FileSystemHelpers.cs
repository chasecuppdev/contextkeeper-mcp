using System.Security.Cryptography;
using System.Text;

namespace ContextKeeper.Utils;

public static class FileSystemHelpers
{
    public static async Task<string> ComputeFileHashAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToBase64String(hash);
    }
    
    public static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
            return 0;
            
        var directory = new DirectoryInfo(path);
        return directory.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }
    
    public static Task<bool> SafeDeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
    
    public static Task<bool> SafeMoveFileAsync(string source, string destination)
    {
        try
        {
            var destDir = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            
            File.Move(source, destination, true);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
    
    public static string GetRelativePath(string basePath, string fullPath)
    {
        var baseUri = new Uri(basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) 
            ? basePath 
            : basePath + Path.DirectorySeparatorChar);
        var fullUri = new Uri(fullPath);
        
        return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString())
            .Replace('/', Path.DirectorySeparatorChar);
    }
    
    public static List<string> GetMatchingFiles(string directory, string pattern)
    {
        var files = new List<string>();
        
        if (!Directory.Exists(directory))
            return files;
        
        try
        {
            // Handle common glob patterns
            if (pattern.StartsWith("*."))
            {
                // Simple extension pattern
                files.AddRange(Directory.GetFiles(directory, pattern, SearchOption.AllDirectories));
            }
            else if (pattern.Contains("*") || pattern.Contains("?"))
            {
                // General glob pattern
                files.AddRange(Directory.GetFiles(directory, pattern, SearchOption.AllDirectories));
            }
            else
            {
                // Exact filename
                var filePath = Path.Combine(directory, pattern);
                if (File.Exists(filePath))
                {
                    files.Add(filePath);
                }
            }
        }
        catch (Exception)
        {
            // Ignore access errors
        }
        
        return files;
    }
}