using System.Text.Json.Serialization;

namespace JobBank1111.Job.WebAPI;

/// <summary>
/// Failure 物件範本
/// 用於表示業務邏輯錯誤與系統錯誤
/// </summary>
public class Failure
{
    /// <summary>
    /// 錯誤代碼（對應 FailureCode 列舉）
    /// </summary>
    public required FailureCode Code { get; init; }

    /// <summary>
    /// 錯誤訊息（給開發者看的詳細訊息）
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// 追蹤識別碼（用於日誌追蹤）
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// 原始例外物件（不序列化到客戶端）
    /// ⚠️ 重要：必須保存原始例外以便除錯
    /// </summary>
    [JsonIgnore]
    public Exception? Exception { get; init; }

    /// <summary>
    /// 結構化資料（可選，用於提供額外的錯誤資訊）
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>
    /// 使用者友善的錯誤訊息（可選）
    /// </summary>
    public string? UserMessage { get; init; }

    /// <summary>
    /// 錯誤發生時間
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// FailureCode 列舉
/// 定義所有可能的錯誤代碼
/// </summary>
public enum FailureCode
{
    /// <summary>
    /// 401 - 未授權存取
    /// </summary>
    Unauthorized,

    /// <summary>
    /// 403 - 禁止存取
    /// </summary>
    Forbidden,

    /// <summary>
    /// 404 - 資源不存在
    /// </summary>
    NotFound,

    /// <summary>
    /// 409 - 重複的 Email
    /// </summary>
    DuplicateEmail,

    /// <summary>
    /// 409 - 資料庫併發衝突
    /// </summary>
    DbConcurrency,

    /// <summary>
    /// 400 - 驗證錯誤
    /// </summary>
    ValidationError,

    /// <summary>
    /// 400 - 無效操作
    /// </summary>
    InvalidOperation,

    /// <summary>
    /// 408 - 請求逾時
    /// </summary>
    Timeout,

    /// <summary>
    /// 500 - 資料庫錯誤
    /// </summary>
    DbError,

    /// <summary>
    /// 500 - 內部伺服器錯誤
    /// </summary>
    InternalServerError,

    /// <summary>
    /// 500 - 未知錯誤
    /// </summary>
    Unknown,

    /// <summary>
    /// 400 - 庫存不足
    /// </summary>
    InsufficientStock,

    /// <summary>
    /// 402 - 付款失敗
    /// </summary>
    PaymentFailed
}

/// <summary>
/// FailureCode 映射器
/// 將 FailureCode 映射至 HTTP 狀態碼
/// </summary>
public static class FailureCodeMapper
{
    public static int ToHttpStatusCode(FailureCode code)
    {
        return code switch
        {
            FailureCode.Unauthorized => 401,
            FailureCode.Forbidden => 403,
            FailureCode.NotFound => 404,
            FailureCode.DuplicateEmail => 409,
            FailureCode.DbConcurrency => 409,
            FailureCode.ValidationError => 400,
            FailureCode.InvalidOperation => 400,
            FailureCode.InsufficientStock => 400,
            FailureCode.Timeout => 408,
            FailureCode.PaymentFailed => 402,
            FailureCode.DbError => 500,
            FailureCode.InternalServerError => 500,
            FailureCode.Unknown => 500,
            _ => 500
        };
    }
}

// ========================================
// 使用範例
// ========================================

/// <summary>
/// Failure 建立範例
/// </summary>
public class FailureExamples
{
    /// <summary>
    /// 範例 1：資源不存在
    /// </summary>
    public static Failure NotFoundExample(Guid id, string? traceId)
    {
        return new Failure
        {
            Code = FailureCode.NotFound,
            Message = $"會員 {id} 不存在",
            TraceId = traceId,
            UserMessage = "找不到指定的會員資料"
        };
    }

    /// <summary>
    /// 範例 2：驗證錯誤
    /// </summary>
    public static Failure ValidationErrorExample(string fieldName, string? traceId)
    {
        return new Failure
        {
            Code = FailureCode.ValidationError,
            Message = $"{fieldName} 驗證失敗",
            TraceId = traceId,
            Data = new Dictionary<string, object>
            {
                ["Field"] = fieldName,
                ["Reason"] = "欄位不可為空"
            },
            UserMessage = "輸入的資料格式不正確"
        };
    }

    /// <summary>
    /// 範例 3：資料庫錯誤（包含例外）
    /// </summary>
    public static Failure DbErrorExample(Exception ex, string? traceId)
    {
        return new Failure
        {
            Code = FailureCode.DbError,
            Message = ex.Message,
            TraceId = traceId,
            Exception = ex,  // ⚠️ 必須保存原始例外
            UserMessage = "系統發生錯誤，請稍後再試"
        };
    }

    /// <summary>
    /// 範例 4：重複資料
    /// </summary>
    public static Failure DuplicateEmailExample(string email, string? traceId)
    {
        return new Failure
        {
            Code = FailureCode.DuplicateEmail,
            Message = $"Email {email} 已被使用",
            TraceId = traceId,
            Data = new Dictionary<string, object>
            {
                ["Email"] = email
            },
            UserMessage = "此 Email 已經被註冊過了"
        };
    }

    /// <summary>
    /// 範例 5：未授權存取
    /// </summary>
    public static Failure UnauthorizedExample(string? traceId)
    {
        return new Failure
        {
            Code = FailureCode.Unauthorized,
            Message = "未授權存取",
            TraceId = traceId,
            UserMessage = "您沒有權限執行此操作"
        };
    }
}
