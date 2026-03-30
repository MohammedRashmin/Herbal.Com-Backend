namespace Web.Com.Helpers;

/// <summary>
/// Inserts f_auto,q_auto into Cloudinary URLs so delivery uses best format (e.g. WebP) and auto quality — reduces file size.
/// </summary>
public static class CloudinaryUrlHelper
{
    private const string UploadSegment = "/image/upload/";
    private const string DeliveryTransform = "f_auto,q_auto/";

    /// <summary>
    /// If the URL is a Cloudinary URL, returns it with f_auto,q_auto for smaller, optimized delivery. Otherwise returns the original URL.
    /// </summary>
    public static string ToDeliveryUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return url ?? string.Empty;

        if (!url.Contains("res.cloudinary.com", StringComparison.OrdinalIgnoreCase))
            return url;

        var idx = url.IndexOf(UploadSegment, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return url;

        // Already has f_auto (or similar) — avoid duplicating
        var afterUpload = idx + UploadSegment.Length;
        if (afterUpload < url.Length && url.AsSpan(afterUpload).StartsWith("f_auto", StringComparison.OrdinalIgnoreCase))
            return url;

        return url.Insert(afterUpload, DeliveryTransform);
    }
}
