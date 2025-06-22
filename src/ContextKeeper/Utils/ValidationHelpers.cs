using System.Text.RegularExpressions;

namespace ContextKeeper.Utils;

public static class ValidationHelpers
{
    public static ValidationResult ValidateMilestone(string milestone, string pattern, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(milestone))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Milestone description cannot be empty"
            };
        }
        
        if (milestone.Length > maxLength)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = $"Milestone description exceeds maximum length of {maxLength} characters"
            };
        }
        
        if (!Regex.IsMatch(milestone, pattern))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = $"Milestone must match pattern: {pattern} (e.g., 'feature-implementation', 'bug-fix-123')"
            };
        }
        
        return new ValidationResult { IsValid = true };
    }
    
    public static bool IsValidPath(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public static string SanitizeFilename(string filename)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = filename;
        
        foreach (var c in invalid)
        {
            sanitized = sanitized.Replace(c, '-');
        }
        
        return sanitized;
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = "";
}