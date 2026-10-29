using System.Collections.Concurrent;
using System.IO;

namespace CYHDocBrandUI.Services;

/// <summary>
/// Production Cache-Buster & Asset Versioning Engine.
/// Automatically appends version timestamps (?v=YYYYMMDDHHmmss) to static CSS and JS files based on their physical file modification times.
/// When files or the application DLL are updated on deploy, version hashes change automatically so browsers refresh instantly without manual hard-reloads.
/// </summary>
public static class AssetVersioning
{
    private static readonly ConcurrentDictionary<string, (string Version, DateTime CachedAtUtc)> _versionCache = new(StringComparer.OrdinalIgnoreCase);
    private static string? _buildId;
    private static string? _contentRoot;

    public static string BuildId
    {
        get
        {
            if (_buildId != null) return _buildId;
            try
            {
                var loc = typeof(AssetVersioning).Assembly.Location;
                if (!string.IsNullOrEmpty(loc) && File.Exists(loc))
                {
                    _buildId = File.GetLastWriteTimeUtc(loc).ToString("yyyyMMddHHmmss");
                    return _buildId;
                }
            }
            catch { }

            _buildId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return _buildId;
        }
    }

    public static void Initialize(string contentRootPath)
    {
        _contentRoot = contentRootPath;
    }

    /// <summary>
    /// Gets the unique timestamp version for a static file relative to wwwroot.
    /// In Development, always checks physical disk. In Production, caches for 30 seconds to minimize disk I/O.
    /// </summary>
    public static string GetFileVersion(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return BuildId;

        var cleanPath = relativePath.TrimStart('/', '\\');
        var qIdx = cleanPath.IndexOf('?');
        if (qIdx >= 0)
        {
            cleanPath = cleanPath.Substring(0, qIdx);
        }

        // Cache lookup (valid for 30 seconds to avoid repeated disk reads during high RPS)
        if (_versionCache.TryGetValue(cleanPath, out var cached) && (DateTime.UtcNow - cached.CachedAtUtc).TotalSeconds < 30)
        {
            return cached.Version;
        }

        try
        {
            var basePath = _contentRoot ?? AppContext.BaseDirectory;
            var wwwroot = Path.Combine(basePath, "wwwroot");
            var fullPath = Path.Combine(wwwroot, cleanPath.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                var version = File.GetLastWriteTimeUtc(fullPath).ToString("yyyyMMddHHmmss");
                _versionCache[cleanPath] = (version, DateTime.UtcNow);
                return version;
            }
        }
        catch { }

        return BuildId;
    }

    /// <summary>
    /// Returns the asset path with automatic ?v=YYYYMMDDHHmmss query parameter.
    /// E.g. "css/glass-theme.css" -> "css/glass-theme.css?v=20261029120612"
    /// </summary>
    public static string Append(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return relativePath;

        var path = relativePath;
        var qIdx = relativePath.IndexOf('?');
        if (qIdx >= 0)
        {
            path = relativePath.Substring(0, qIdx);
        }

        var version = GetFileVersion(path);
        return $"{path}?v={version}";
    }
}
