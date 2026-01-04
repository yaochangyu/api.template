using Flurl;

namespace JobBank1111.Testing.Common;

public class UrlBuilder
{
    private readonly string _baseUrl;
    private readonly string _initialPath;
    private readonly Dictionary<string, string> _queryParams;

    public UrlBuilder(string baseUrl, string initialPath)
    {
        this._baseUrl = baseUrl;
        this._initialPath = initialPath;

        var uri = new Uri(baseUrl + initialPath);
        var queryParams = Url.ParseQueryParams(uri.Query);
        this._queryParams = queryParams.ToDictionary(p => p.Name,
                                                     p => p.Value.ToString());
    }

    public void AddParameter(string key, string value)
    {
        if (this._queryParams.ContainsKey(key))
        {
            this._queryParams[key] = value;
        }
        else
        {
            this._queryParams.Add(key, value);
        }
    }

    public string BuildUrl()
    {
        try
        {
            var url = this._baseUrl
                .AppendPathSegment(this._initialPath.Split('?')[0])
                .SetQueryParams(this._queryParams);

            return url.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"URL 建立失敗: {ex.Message}");
        }
    }
}