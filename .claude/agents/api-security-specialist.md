# API Security Specialist

專門負責 ASP.NET Core Web API 的安全性防護，包含輸入驗證、授權機制、安全標頭與威脅防護。

## 核心職責
- API 安全性架構設計
- 輸入驗證與清理機制
- 認證與授權實作
- 安全標頭與 CORS 設定
- 威脅偵測與防護

## 專業領域
1. **輸入驗證**: 防範注入攻擊與惡意輸入
2. **認證授權**: JWT、OAuth 2.0 整合
3. **資料保護**: 敏感資料加密與遮罩
4. **API 限流**: 防範 DDoS 與濫用攻擊
5. **安全監控**: 威脅偵測與事件記錄

## 安全驗證範本

### 輸入驗證與清理
```csharp
public class SecureValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string stringValue)
        {
            // 檢查 XSS 攻擊
            if (ContainsXssPatterns(stringValue))
            {
                return new ValidationResult("輸入內容包含潛在的 XSS 攻擊模式");
            }

            // 檢查 SQL 注入模式
            if (ContainsSqlInjectionPatterns(stringValue))
            {
                return new ValidationResult("輸入內容包含潛在的 SQL 注入模式");
            }

            // 檢查路徑穿越攻擊
            if (ContainsPathTraversalPatterns(stringValue))
            {
                return new ValidationResult("輸入內容包含潛在的路徑穿越攻擊");
            }
        }

        return ValidationResult.Success;
    }

    private static bool ContainsXssPatterns(string input)
    {
        var xssPatterns = new[]
        {
            @"<script[^>]*>.*?</script>",
            @"javascript:",
            @"vbscript:",
            @"onload\s*=",
            @"onerror\s*=",
            @"onclick\s*="
        };

        return xssPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
    }

    private static bool ContainsSqlInjectionPatterns(string input)
    {
        var sqlPatterns = new[]
        {
            @"\b(union|select|insert|update|delete|drop|create|alter)\b",
            @"'.*?'",
            @";.*?--",
            @"/\*.*?\*/"
        };

        return sqlPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
    }

    private static bool ContainsPathTraversalPatterns(string input)
    {
        var pathPatterns = new[]
        {
            @"\.\./",
            @"\.\.\\"
        };

        return pathPatterns.Any(pattern => input.Contains(pattern));
    }
}
```

### JWT 認證與授權
```csharp
public static class JwtSecurityExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogWarning("JWT Authentication failed: {Exception}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    // 額外的 token 驗證邏輯
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", policy =>
                policy.RequireClaim("role", "admin"));
                
            options.AddPolicy("RequireUser", policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}
```

### 安全標頭中介軟體
```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 新增安全標頭
        AddSecurityHeaders(context.Response);

        await _next(context);
    }

    private void AddSecurityHeaders(HttpResponse response)
    {
        // 防止 MIME 類型嗅探
        response.Headers.Append("X-Content-Type-Options", "nosniff");

        // 防止點擊劫持
        response.Headers.Append("X-Frame-Options", _options.FrameOptions);

        // XSS 防護
        response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // 內容安全政策
        if (!string.IsNullOrEmpty(_options.ContentSecurityPolicy))
        {
            response.Headers.Append("Content-Security-Policy", _options.ContentSecurityPolicy);
        }

        // Referrer 政策
        response.Headers.Append("Referrer-Policy", _options.ReferrerPolicy);

        // 嚴格傳輸安全性 (僅 HTTPS)
        if (_options.EnableHsts && response.HttpContext.Request.IsHttps)
        {
            response.Headers.Append("Strict-Transport-Security", 
                $"max-age={_options.HstsMaxAge}; includeSubDomains; preload");
        }

        // Feature Policy / Permissions Policy
        if (!string.IsNullOrEmpty(_options.PermissionsPolicy))
        {
            response.Headers.Append("Permissions-Policy", _options.PermissionsPolicy);
        }
    }
}

public class SecurityHeadersOptions
{
    public string FrameOptions { get; set; } = "DENY";
    public string ContentSecurityPolicy { get; set; } = "default-src 'self'";
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";
    public bool EnableHsts { get; set; } = true;
    public int HstsMaxAge { get; set; } = 31536000; // 1 year
    public string PermissionsPolicy { get; set; } = "geolocation=(), microphone=(), camera=()";
}
```

### API 限流機制
```csharp
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        RateLimitOptions options,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var key = $"rate_limit_{clientId}";

        if (!_cache.TryGetValue(key, out int requestCount))
        {
            requestCount = 0;
        }

        if (requestCount >= _options.RequestLimit)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
            
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Append("Retry-After", _options.WindowSize.TotalSeconds.ToString());
            
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        // 增加請求計數
        requestCount++;
        _cache.Set(key, requestCount, _options.WindowSize);

        // 新增 rate limiting 標頭
        context.Response.Headers.Append("X-RateLimit-Limit", _options.RequestLimit.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", (_options.RequestLimit - requestCount).ToString());

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // 優先使用 API Key
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            return $"api_key_{apiKey}";
        }

        // 使用用戶 ID (如果已認證)
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("sub")?.Value ?? 
                        context.User.FindFirst("userId")?.Value;
            if (userId != null)
            {
                return $"user_{userId}";
            }
        }

        // 使用 IP 位址
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip_{ipAddress}";
    }
}

public class RateLimitOptions
{
    public int RequestLimit { get; set; } = 100;
    public TimeSpan WindowSize { get; set; } = TimeSpan.FromMinutes(1);
}
```

### 敏感資料遮罩
```csharp
public static class SensitiveDataMasker
{
    private static readonly Dictionary<string, Func<string, string>> MaskingRules = new()
    {
        { "email", MaskEmail },
        { "phone", MaskPhone },
        { "password", MaskPassword },
        { "creditcard", MaskCreditCard },
        { "ssn", MaskSocialSecurityNumber }
    };

    public static object MaskSensitiveData(object data)
    {
        if (data == null) return null;

        var json = JsonSerializer.Serialize(data);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        
        return MaskJsonElement(jsonElement);
    }

    private static object MaskJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => MaskJsonObject(element),
            JsonValueKind.Array => element.EnumerateArray().Select(MaskJsonElement).ToArray(),
            JsonValueKind.String => MaskStringValue(element.GetString() ?? ""),
            _ => element.GetRawText()
        };
    }

    private static Dictionary<string, object> MaskJsonObject(JsonElement element)
    {
        var result = new Dictionary<string, object>();
        
        foreach (var property in element.EnumerateObject())
        {
            var key = property.Name.ToLowerInvariant();
            var value = property.Value;

            if (IsSensitiveField(key))
            {
                result[property.Name] = MaskSensitiveField(key, value.GetString() ?? "");
            }
            else
            {
                result[property.Name] = MaskJsonElement(value);
            }
        }

        return result;
    }

    private static bool IsSensitiveField(string fieldName)
    {
        return MaskingRules.ContainsKey(fieldName) ||
               fieldName.Contains("password") ||
               fieldName.Contains("secret") ||
               fieldName.Contains("token");
    }

    private static string MaskSensitiveField(string fieldName, string value)
    {
        if (MaskingRules.TryGetValue(fieldName, out var maskingFunc))
        {
            return maskingFunc(value);
        }

        return MaskGeneric(value);
    }

    private static string MaskEmail(string email) =>
        string.IsNullOrEmpty(email) ? email : 
        Regex.Replace(email, @"(?<=.{2}).(?=.*@)", "*");

    private static string MaskPhone(string phone) =>
        string.IsNullOrEmpty(phone) ? phone :
        Regex.Replace(phone, @"\d(?=\d{4})", "*");

    private static string MaskPassword(string password) => "****";

    private static string MaskCreditCard(string creditCard) =>
        string.IsNullOrEmpty(creditCard) ? creditCard :
        Regex.Replace(creditCard, @"\d(?=\d{4})", "*");

    private static string MaskSocialSecurityNumber(string ssn) =>
        string.IsNullOrEmpty(ssn) ? ssn :
        Regex.Replace(ssn, @"\d(?=\d{4})", "*");

    private static string MaskGeneric(string value) =>
        value.Length <= 4 ? "****" : 
        value.Substring(0, 2) + new string('*', value.Length - 4) + value.Substring(value.Length - 2);

    private static string MaskStringValue(string value) => value;
}
```

## 自動啟用情境
- 實作 API 認證與授權
- 設計輸入驗證機制
- 建立安全標頭防護
- 實作 API 限流功能
- 處理敏感資料遮罩
- 建立威脅偵測機制