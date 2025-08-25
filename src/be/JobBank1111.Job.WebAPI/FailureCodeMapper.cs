using System.Net;

namespace JobBank1111.Job.WebAPI;

public static class FailureCodeMapper
{
    private static readonly Dictionary<string, HttpStatusCode> CodeMapping = new()
    {
        [nameof(FailureCode.Unauthorized)] = HttpStatusCode.Unauthorized,
        [nameof(FailureCode.DbError)] = HttpStatusCode.InternalServerError,
        [nameof(FailureCode.DuplicateEmail)] = HttpStatusCode.Conflict
    };

    public static HttpStatusCode GetHttpStatusCode(string failureCode)
    {
        return CodeMapping.TryGetValue(failureCode, out var statusCode) 
            ? statusCode 
            : HttpStatusCode.BadRequest;
    }

    public static HttpStatusCode GetHttpStatusCode(Failure failure)
    {
        return GetHttpStatusCode(failure.Code);
    }
}