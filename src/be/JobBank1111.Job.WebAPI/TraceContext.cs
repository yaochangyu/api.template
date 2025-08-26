namespace JobBank1111.Job.WebAPI;
// 透過 IContextSetter 設定用戶資訊
// 透過 IContextGetter 取得用戶資訊

public record TraceContext
{
    public string TraceId { get; init; }

    public string UserId { get; init; }
}