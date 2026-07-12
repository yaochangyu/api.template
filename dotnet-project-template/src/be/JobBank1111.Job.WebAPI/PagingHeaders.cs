namespace JobBank1111.Job.WebAPI;

/// <summary>
/// 分頁相關 Header 解析（v1/v2 共用）。
/// 行為統一：Header 不存在或格式錯誤時一律回預設值（TryParse＋fallback）。
/// </summary>
public static class PagingHeaders
{
    public const string PageSizeHeaderName = "x-page-size";
    public const string PageIndexHeaderName = "x-page-index";
    public const string NextPageTokenHeaderName = "x-next-page-token";
    public const string CacheControlHeaderName = "cache-control";

    public static int GetPageSize(this HttpRequest request, int defaultValue = 10)
    {
        return TryParseInt(request, PageSizeHeaderName, defaultValue);
    }

    public static int GetPageIndex(this HttpRequest request, int defaultValue = 0)
    {
        return TryParseInt(request, PageIndexHeaderName, defaultValue);
    }

    public static string? GetNextPageToken(this HttpRequest request)
    {
        return request.Headers.TryGetValue(NextPageTokenHeaderName, out var value)
            ? value.FirstOrDefault()
            : null;
    }

    public static bool GetNoCache(this HttpRequest request, bool defaultValue = false)
    {
        if (request.Headers.TryGetValue(CacheControlHeaderName, out var value)
            && bool.TryParse(value.FirstOrDefault(), out var noCache))
        {
            return noCache;
        }

        return defaultValue;
    }

    private static int TryParseInt(HttpRequest request, string headerName, int defaultValue)
    {
        if (request.Headers.TryGetValue(headerName, out var value)
            && int.TryParse(value.FirstOrDefault(), out var parsed))
        {
            return parsed;
        }

        return defaultValue;
    }
}
