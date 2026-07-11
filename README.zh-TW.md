# api.template — WebAPI 開發範本

**語言**: [English](./README.md) | 繁體中文

.NET WebAPI 開發範本，實踐 **Clean Architecture**、**分層架構** 與 **Result Pattern** 的最佳實踐。

## 📋 快速開始

### 前置需求
- .NET 8+ SDK
- Docker & Docker Compose（用於資料庫與快取測試環境）
- Git

### 基本步驟

1. **檢出專案**
   ```bash
   git clone https://github.com/yaochangyu/api.template.git
   cd api.template/dotnet-project-template
   ```

2. **設定環境變數**
   ```bash
   cp env/local.env .env
   # 編輯 .env 以設定本機資料庫連接字串、Redis 位址等
   ```

3. **啟動開發環境**
   ```bash
   docker-compose up -d      # 啟動 SQL Server、Redis 等依賴
   dotnet build               # 編譯解決方案
   dotnet run --project src/be/JobBank1111.Job.WebAPI
   ```

4. **驗證 API 是否運行**
   ```bash
   curl http://localhost:5000/api/health
   ```

5. **執行測試**
   ```bash
   dotnet test                # 執行所有單元與整合測試
   ```

## 📁 目錄結構

```
api.template/
├── .claude/                          # 開發工具與 AI 助理指南
│   ├── CLAUDE.md                     # AI 助理行為規則與計畫管理
│   ├── development-rules.md          # 開發規則與最佳實踐
│   ├── decision-framework.md         # API 開發流程決策邏輯
│   ├── skills/                       # 開發助手技能定義（16 個）
│   │   ├── api-development/          # API 端點設計與決策邏輯
│   │   ├── handler/                  # 業務邏輯層實現
│   │   ├── repository-design/        # 資料存取層設計
│   │   ├── error-handling/           # 統一錯誤處理模式
│   │   ├── middleware/               # HTTP 中介軟體實現
│   │   ├── ef-core/                  # EF Core 最佳化指南
│   │   ├── bdd-testing/              # BDD 整合測試（Reqnroll/Gherkin）
│   │   └── [其他 9 個技能]
│   └── agents/                       # AI Agent 工作流（架構檢視、功能開發等）
│
├── dotnet-project-template/          # 主要 .NET 專案
│   ├── src/be/                       # 後端程式碼
│   │   ├── JobBank1111.Infrastructure/    # 共用基礎設施（快取、日誌、TraceContext）
│   │   ├── JobBank1111.Job.Contract/      # API 契約 & 自動生成的 Client
│   │   ├── JobBank1111.Job.DB/            # EF Core DbContext & Entity
│   │   ├── JobBank1111.Job.WebAPI/        # API Controller 與中介軟體
│   │   ├── JobBank1111.Job.Test/          # 單元測試 (Xunit)
│   │   └── JobBank1111.Job.IntegrationTest/ # 整合測試 (Reqnroll/BDD)
│   │
│   ├── doc/                          # API 規格文檔
│   │   └── openapi.yml               # OpenAPI 規格（Swagger）
│   │
│   ├── env/                          # 環境配置
│   │   ├── .template-config.json     # 範本配置
│   │   └── local.env                 # 本機環境變數
│   │
│   ├── k8s/                          # Kubernetes 部署配置
│   ├── docker-compose.yml            # 本機開發環境（DB、Redis）
│   ├── Taskfile.yml                  # 常用任務（編譯、測試、部署）
│   └── best-practices.md             # 專案最佳實踐指南
│
├── CLAUDE.md                         # AI 助理行為規則與計畫管理
├── tree.md                           # 完整檔案清單
└── .archive/                         # 已完成的計畫文檔（用於參考）
```

## 🏗️ 核心架構

### 分層設計

```
HTTP Request
     ↓
[ Middleware Layer ]      # 日誌、認證、TraceContext、錯誤處理
     ↓
[ Controller Layer ]      # HTTP 路由與請求驗證（MemberController.cs）
     ↓
[ Handler Layer ]         # 業務邏輯（MemberHandler.cs）
     ↓
[ Repository Layer ]      # 資料存取抽象（IMemberRepository）
     ↓
[ EF Core DbContext ]     # ORM 實現
     ↓
[ Database ]              # SQL Server / PostgreSQL
```

### 核心概念

#### 1. **API 開發方式**

根據專案階段選擇合適的開發方式（**必須擇一，不得混用**）：

| 方式 | 何時使用 | 代表檔案 |
|------|--------|--------|
| **API First** | API 規格已確定，自動產生 Controller | `MemberV1ControllerImpl.cs` |
| **Code First** | 直接實作 Controller，無規格自動生成 | `MemberController.cs` |

👉 詳細決策邏輯：見 [.claude/decision-framework.md](./.claude/decision-framework.md#api開發流程決策)

#### 2. **Result Pattern（統一結果型態）**

所有 API 端點返回統一的 `Result<T>` 結構：

```csharp
{
  "isSuccess": true,
  "data": { ... },
  "failure": null,
  "traceId": "550e8400-e29b-41d4-a716-446655440000"
}
```

- **優點**：統一錯誤處理、完整的追蹤上下文、自動日誌記錄
- **實現**：見 [/error-handling SKILL](./.claude/skills/error-handling/SKILL.md)

#### 3. **TraceContext（請求追蹤）**

每個請求自動分配唯一的 TraceId，貫穿整個調用鏈：

```csharp
public class TraceContext
{
    public string TraceId { get; set; }  // 全局唯一識別符
    public string UserId { get; set; }   // 當前使用者
    public DateTime RequestTime { get; set; }
}
```

便利於日誌聚合、效能分析、故障診斷。

#### 4. **不可變物件**

使用 C# record 類型與 init 屬性，確保物件創建後不可修改：

```csharp
public record MemberResponse(
    int Id,
    string Name,
    string Email
);
```

#### 5. **非同步 I/O**

所有資料庫、網路操作都使用 async/await：

```csharp
public async Task<Result<MemberResponse>> GetMemberAsync(int id)
{
    var member = await _repository.GetByIdAsync(id);
    return member != null 
        ? Result.Ok(new MemberResponse(member.Id, member.Name, member.Email))
        : Result.Fail<MemberResponse>("會員不存在");
}
```

## 🛠️ 開發工作流

### 典型的 API 端點實現流程

假設要實作「取得會員列表」端點：

1. **選擇開發方式**（API First 或 Code First）
   ```bash
   /api-development
   ```

2. **設計資料存取層**
   ```bash
   /repository-design
   ```

3. **實作業務邏輯層**
   ```bash
   /handler
   ```

4. **實作 Controller**
   ```bash
   /handler   # 同時產生 Controller 參考
   ```

5. **實作 BDD 測試**
   ```bash
   /bdd-testing
   ```

6. **檢查完整性**
   ```bash
   dotnet test
   dotnet build
   ```

## 📖 進階指南

根據工作項查閱對應文檔：

| 工作項 | 相關文檔 | 位置 |
|--------|--------|------|
| 理解開發工作流 | AI 助理行為規則與 SKILL 使用指南 | [CLAUDE.md](./CLAUDE.md) |
| 設計 API 端點 | API 開發決策框架 | [.claude/decision-framework.md](./.claude/decision-framework.md) |
| 實作 Controller | 開發規則與最佳實踐 | [.claude/development-rules.md](./.claude/development-rules.md) |
| 撰寫整合測試 | BDD 測試指南 | [.claude/skills/bdd-testing/SKILL.md](./.claude/skills/bdd-testing/SKILL.md) |
| 快取設計 | 快取策略決策 | [.claude/decision-framework.md#快取策略設計](./.claude/decision-framework.md) |
| 安全檢查 | 安全掃描工具 | [.claude/skills/security-*](./.claude/skills/) |

## 📚 重要文檔

- **[CLAUDE.md](./CLAUDE.md)** — AI 助理行為規則、計畫管理流程、進階主題索引
- **[.claude/development-rules.md](./.claude/development-rules.md)** — 開發規則與最佳實踐
- **[dotnet-project-template/best-practices.md](./dotnet-project-template/best-practices.md)** — .NET 專案最佳實踐
- **[dotnet-project-template/docs/development/reqnroll-best-practices.md](./dotnet-project-template/docs/development/reqnroll-best-practices.md)** — BDD/Gherkin 測試指南

## 🔧 常用命令

```bash
# 開發
dotnet build                                    # 編譯整個解決方案
dotnet run --project src/be/JobBank1111.Job.WebAPI   # 啟動 API 伺服器
dotnet watch run --project src/be/JobBank1111.Job.WebAPI  # 熱重載模式

# 測試
dotnet test                                     # 執行所有測試（單元 + 整合）
dotnet test --filter FullyQualifiedName~Member # 執行特定測試

# 資料庫遷移（使用 EF Core）
dotnet ef migrations add CreateMemberTable --project src/be/JobBank1111.Job.DB
dotnet ef database update --project src/be/JobBank1111.Job.DB

# Docker
docker-compose up -d                            # 啟動本機開發環境
docker-compose down                             # 關閉開發環境
docker-compose logs -f                          # 查看容器日誌
```

## 🎯 開發政策

### 必須遵守

- ✅ **分層架構**：Controller → Handler → Repository → DbContext
- ✅ **不可變物件**：使用 record 與 init 屬性
- ✅ **異步操作**：所有 I/O 都使用 async/await
- ✅ **Result Pattern**：統一的成功/失敗型態
- ✅ **TraceContext**：每個請求攜帶 TraceId
- ✅ **BDD 測試**：整合測試使用 Gherkin/Reqnroll

### 禁止

- ❌ 混用 API First 和 Code First 開發方式
- ❌ 跳過 async/await（同步 I/O 會阻塞執行緒）
- ❌ 直接在 Controller 撰寫業務邏輯（違反分層設計）
- ❌ Mock 資料庫（整合測試應使用真實 Docker 容器）

## 📞 疑問與支援

- 工作流疑問：查閱 [CLAUDE.md](./CLAUDE.md) 與 SKILL 文檔
- 開發決策：參考 [.claude/decision-framework.md](./.claude/decision-framework.md)
- 技術問題：查看 [dotnet-project-template/best-practices.md](./dotnet-project-template/best-practices.md)

---

**版本**: api.template v1.0  
**最後更新**: 2026-07-11  
**文件用途**: 適合人類與 AI Agent 閱讀
