using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JobBank1111.Job.WebAPI;

/// <summary>
/// Custom JSON output formatter using synchronous serialization to avoid .NET 10 PipeWriter.UnflushedBytes issue
/// with System.Text.Json async serialization in SystemTextJsonOutputFormatter.
/// </summary>
public class SynchronousJsonOutputFormatter : TextOutputFormatter
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public SynchronousJsonOutputFormatter(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions();
        SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));
        SupportedEncodings.Add(System.Text.Encoding.UTF8);
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, System.Text.Encoding selectedEncoding)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var response = context.HttpContext.Response;

        // Use synchronous serialization instead of async to avoid PipeWriter.UnflushedBytes
        var json = JsonSerializer.Serialize(context.Object, _jsonSerializerOptions);
        var bytes = selectedEncoding.GetBytes(json);

        await response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
}
