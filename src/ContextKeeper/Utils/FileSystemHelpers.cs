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
    
    public static async Task<bool> SafeDeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    public static async Task<bool> SafeMoveFileAsync(string source, string destination)
    {
        try
        {
            var destDir = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            
            File.Move(source, destination, true);
            return true;
        }
        catch
        {
            return false;
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
}