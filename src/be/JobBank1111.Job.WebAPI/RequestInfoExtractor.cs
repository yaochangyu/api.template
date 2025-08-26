using System.Text;
using System.Text.Json;

namespace JobBank1111.Job.WebAPI;

public static class RequestInfoExtractor
{
    public static async Task<object> ExtractRequestInfoAsync(HttpContext context, JsonSerializerOptions jsonOptions)
    {
        var parameters = new Dictionary<string, object>();

        // 1. Route parameters
        if (context.Request.RouteValues.Count > 0)
        {
            parameters["Route"] = context.Request.RouteValues.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        // 2. Query parameters
        if (context.Request.Query.Count > 0)
        {
            parameters["Query"] = context.Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        }

        // 3. Headers (excluding sensitive headers)
        var headers = context.Request.Headers
            .Where(h => !IsSensitiveHeader(h.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        
        if (headers.Count > 0)
        {
            parameters["Headers"] = headers;
        }

        // 4. Request body (for POST/PUT/PATCH requests)
        if (context.Request.ContentLength > 0 && 
            (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH"))
        {
            var body = await ReadRequestBodyAsync(context, jsonOptions);
            if (body != null)
            {
                parameters["Body"] = body;
            }
        }

        // 5. Basic request info
        parameters["Method"] = context.Request.Method;
        parameters["Path"] = context.Request.Path.ToString();
        parameters["ContentType"] = context.Request.ContentType ?? "";
        parameters["ContentLength"] = context.Request.ContentLength ?? 0;

        return parameters;
    }

    private static async Task<object> ReadRequestBodyAsync(HttpContext context, JsonSerializerOptions jsonOptions)
    {
        try
        {
            // Enable buffering so the request body can be read multiple times
            context.Request.EnableBuffering();
            
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            
            // Reset the stream position for subsequent middleware
            context.Request.Body.Position = 0;
            
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            // Try to parse as JSON, if it fails return as string
            try
            {
                var contentType = context.Request.ContentType ?? "";
                if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    using var document = JsonDocument.Parse(body);
                    return JsonSerializer.Deserialize<object>(document.RootElement, jsonOptions);
                }
            }
            catch
            {
                // If JSON parsing fails, return as string
            }

            return body;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "authorization",
            "cookie",
            "x-api-key",
            "x-auth-token",
            "authentication",
            "proxy-authorization"
        };

        return sensitiveHeaders.Contains(headerName.ToLowerInvariant());
    }
}