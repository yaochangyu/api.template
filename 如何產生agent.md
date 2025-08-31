# 如何產生 Agent - ASP.NET Core Web API 程式碼規則框架

基於 SuperClaude Framework 的架構分析，本文檔提供三種將 ASP.NET Core Web API 程式碼規則轉換為指令或代理人的解決方案。

## SuperClaude Framework 架構分析

### 核心設計理念
- 一個「元程式設計配置框架」，將 Claude Code 轉換為結構化的開發平台
- 透過行為指令注入和組件編排實現系統化工作流程自動化

### 關鍵架構組件

1. **Slash Commands (22 個)**
   - 以 `/sc:` 為前綴
   - 涵蓋完整開發生命週期
   - 提供清潔、有組織的指令結構

2. **專業代理人 (14 個)**
   - 領域特定專業知識
   - 包含安全工程師、前端架構師等
   - 基於情境的自動協調

3. **行為模式 (6 種)**
   - 適應不同情境的模式：
     - 腦力激盪、商業面板、編排
     - Token 效率、任務管理、內省

4. **MCP Server 整合 (6 個伺服器)**
   - Context7: 文檔更新
   - Sequential: 複雜分析
   - Magic: UI 組件生成
   - Playwright: 瀏覽器測試
   - Morphllm: 批量轉換
   - Serena: 會話持久化

## 方案一：Command 模式 - Slash Commands

建立 `.claude/commands/` 目錄與指令：

### WebAPI 系列指令

```markdown
<!-- .claude/commands/webapi.md -->
# /webapi: 系列指令

## /webapi:handler [實體名稱]
產生符合專案規範的 Handler 類別
- 使用 Result Pattern
- 整合 TraceContext
- 包含驗證邏輯和錯誤處理

## /webapi:controller [實體名稱]
產生 Controller 實作類別
- 繼承自動產生的介面
- HTTP 標頭參數處理
- Result 轉換為 ActionResult

## /webapi:middleware [中介軟體名稱]
產生 Middleware 類別
- 遵循專案中介軟體模式
- 包含日誌記錄和錯誤處理
- TraceContext 整合

## /webapi:repository [實體名稱]
產生 Repository 類別
- Entity Framework 整合
- 快取機制
- 錯誤處理模式
```

## 方案二：Agent 模式 - 專業代理人

建立 `.claude/agents/` 目錄與專業代理人：

### Clean Architecture Engineer

```markdown
<!-- .claude/agents/clean-architecture-engineer.md -->
# Clean Architecture Engineer

專門依據 Clean Architecture 原則與專案規範產生 ASP.NET Core Web API 程式碼。

## 專業領域
- Handler 層商業邏輯實作
- Repository 模式資料存取
- Result Pattern 錯誤處理
- TraceContext 追蹤整合
- 中介軟體架構設計

## 核心能力
1. **遵循專案規範**: 嚴格按照 CLAUDE.md 規範實作
2. **錯誤處理**: 統一使用 Failure 物件和 Result Pattern
3. **追蹤整合**: 自動整合 TraceContext 和日誌記錄
4. **驗證邏輯**: 實作連續驗證模式

## 自動啟用情境
- 建立新的實體處理邏輯
- 實作 API 端點
- 設計中介軟體
- 重構業務邏輯
```

### Middleware Architect

```markdown
<!-- .claude/agents/middleware-architect.md -->
# Middleware Architect

專門設計和實作 ASP.NET Core 中介軟體，遵循專案的分層架構和職責分離原則。

## 核心職責
- 設計中介軟體管線架構
- 實作跨領域關注點處理
- 整合 TraceContext 和日誌系統
- 錯誤處理和安全防護

## 設計原則
1. **專一職責**: 每個中介軟體專注單一關注點
2. **避免重複**: 通過管線設計避免重複處理
3. **統一格式**: 所有請求資訊使用相同結構
4. **效能考量**: 只在需要時擷取請求內容

## 標準模板
- ExceptionHandlingMiddleware 模式
- TraceContextMiddleware 模式
- 日誌記錄中介軟體模式
```

## 方案三：Framework 模式 - 完整框架

建立類似 SuperClaude 的完整框架：

### JobBank API Framework

```markdown
<!-- .claude/framework/jobbank-framework.md -->
# JobBank API Framework

基於 Clean Architecture 的 ASP.NET Core Web API 開發框架

## 框架架構

### 1. Slash Commands (/jb:*)
- `/jb:handler` - Handler 產生
- `/jb:controller` - Controller 產生  
- `/jb:middleware` - Middleware 產生
- `/jb:repository` - Repository 產生
- `/jb:test` - 測試類別產生
- `/jb:migration` - EF Migration 產生

### 2. 專業代理人
- clean-architecture-engineer: 業務邏輯實作
- middleware-architect: 中介軟體設計
- api-security-specialist: 安全性實作
- performance-optimizer: 效能最佳化
- testing-engineer: 測試程式碼產生

### 3. 行為模式
- development-mode: 開發階段快速產生程式碼
- refactoring-mode: 重構現有程式碼
- testing-mode: 專注於測試驗證
- documentation-mode: API 文件產生

### 4. 程式碼規則引擎
- Result Pattern 強制檢查
- TraceContext 整合驗證
- 命名規範檢查
- 架構守則驗證
```

## 推薦實作方案

### 漸進式實作策略

1. **階段一**：先實作 Command 模式，立即可用
2. **階段二**：加入專業代理人，提升智能化
3. **階段三**：建構完整框架，達到 SuperClaude 等級

### 立即開始方案：Command + Agent 混合

結合兩種模式的優點：
- **指令提供快速操作**: 透過 slash commands 快速產生程式碼
- **代理人提供智能判斷**: 自動選擇最適合的實作模式
- **框架確保一致性**: 統一的程式碼規範和最佳實務

## 實作範例

### Handler 產生器範例

```csharp
// HandlerTemplate.cs
public static class HandlerTemplate
{
    public static string GetTemplate(string name, string entity) => $$"""
using CSharpFunctionalExtensions;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI.{{entity}};

public class {{name}}Handler(
    {{name}}Repository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<{{name}}Handler> logger)
{
    public async Task<Result<{{entity}}, Failure>>
        CreateAsync(Create{{entity}}Request request,
                    CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        // 驗證邏輯
        var validateResult = ValidateRequest(request);
        if (validateResult.IsFailure)
        {
            return validateResult;
        }

        var insertResult = await repository.CreateAsync(request, cancel);
        if (insertResult.IsFailure)
        {
            return Result.Failure<{{entity}}, Failure>(insertResult.Error);
        }

        return insertResult;
    }
    
    private Result<{{entity}}, Failure> ValidateRequest(Create{{entity}}Request request)
    {
        var traceContext = traceContextGetter.Get();
        
        // 實作驗證邏輯
        if (string.IsNullOrEmpty(request.Name))
        {
            return Result.Failure<{{entity}}, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Name 不能為空",
                TraceId = traceContext?.TraceId
            });
        }
        
        return Result.Success<{{entity}}, Failure>(null);
    }
}
""";
}
```

### Taskfile.yml 整合

```yaml
# 新增到 Taskfile.yml
tasks:
  codegen-handler:
    desc: "生成新的 Handler 類別"
    cmds:
      - echo "產生 {{.NAME}}Handler 中..."
      - dotnet run --project tools/CodeGenerator -- handler --name {{.NAME}} --entity {{.ENTITY}}
  
  codegen-middleware:
    desc: "生成新的 Middleware"
    cmds:
      - echo "產生 {{.NAME}}Middleware 中..."
      - dotnet run --project tools/CodeGenerator -- middleware --name {{.NAME}}
  
  codegen-controller:
    desc: "生成 Controller 實作"
    cmds:
      - echo "產生 {{.NAME}}Controller 實作中..."
      - dotnet run --project tools/CodeGenerator -- controller --name {{.NAME}}
```

## 使用方式

### Command 模式使用
```bash
# 使用 Taskfile 指令
task codegen-handler NAME=Product ENTITY=Product

# 或直接使用 Claude Code slash command
/webapi:handler Product
```

### Agent 模式使用
```
# 直接描述需求，代理人自動判斷實作方式
請為產品管理建立完整的 Handler 和 Controller

# 或明確啟用特定代理人
@clean-architecture-engineer 實作產品新增功能
```

### Framework 模式使用
```
# 使用框架指令
/jb:handler Product
/jb:controller Product
/jb:test Product

# 切換行為模式
/jb:mode development
/jb:mode refactoring
```

## 效益與優勢

1. **提高開發效率**: 快速產生符合規範的程式碼
2. **確保程式碼一致性**: 統一的架構模式和最佳實務
3. **減少人為錯誤**: 自動整合 TraceContext 和錯誤處理
4. **提升程式碼品質**: 強制遵循 Clean Architecture 原則
5. **簡化維護工作**: 標準化的程式碼結構便於維護

透過這套框架，開發團隊可以更專注於業務邏輯的實作，而不需要重複處理基礎設施程式碼的細節。