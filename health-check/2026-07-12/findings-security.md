# 安全掃描結果（2026-07-12，security-scan agent／sonnet 實跑，主對話代筆落地）

**摘要：P0=0（無真實憑證洩漏）、P1=0、P2=1、P3=4（皆範本/框架預設值）**

## 1. Secrets 掃描
| 發現 | 證據 | 嚴重度 |
|---|---|---|
| 明碼密碼 `Password=pass@w0rd1~` | `dotnet-project-template/env/local.env:2`；Server=localhost、docker-compose 無 SQL Server 容器 → 本機開發範本預設密碼 | P3（範本預設值） |
| .mcp.json API keys | 全為空字串或 `<YOUR_TOKEN>` 佔位符，非真實憑證 | P3 |
| 雲端金鑰樣式全庫 grep（AKIA/AIza/sk-/sk_live_/ghp_/BEGIN PRIVATE KEY） | 僅命中 `.claude/skills/security-check-secrets/` 文件內的 AWS 官方 example key 與 regex 說明 | 無 P0 |
| appsettings*.json | 僅 Logging 設定，無憑證 | — |
| docker-compose.yml（seq/redis/redis-admin） | 未設帳密（redis 無 requirepass） | P3（本機開發便利） |

## 2. .gitignore 覆蓋
- 根層 `.gitignore` 排除 `.omc/`、`.omo/`；`dotnet-project-template/.gitignore` 涵蓋標準 VS 規則。
- `git status --ignored` × `git ls-files` 交叉比對：bin/obj、`*.DotSettings.user`、`.idea/`、`logs/`、test-output.log 均正確忽略且未入版。唯一命中 `.env` 樣式者為刻意提供的 `env/local.env`。**無漏網之魚。**

## 3. 安全配置
- `Program.cs:84` UseHttpsRedirection 已啟用。
- CORS：全庫 *.cs 無任何 CORS 設定 → 不存在 AllowAnyOrigin；中性觀察（範本未內建，部署時需自行加白名單）。
- OpenAPI 端點（`Program.cs:79-82`）僅 Development 開放，符合最佳實踐。
- `appsettings.json` `"AllowedHosts": "*"` — ASP.NET Core 框架預設值。P3。
- ExceptionHandlingMiddleware 深查（主對話 2026-07-12 補驗）：
  - ✅ StackTrace 不洩漏：`Failure.Exception` 有 `[JsonIgnore]`（`Failure.cs:40`，註解明寫「不回傳給 Web API」）。
  - ⚠️ **P2**：`Message = exception.Message`（`ExceptionHandlingMiddleware.cs:79`）將原始例外訊息原樣回傳用戶端，且不分環境——Production 下可能外洩內部細節（連線字串片段、SQL 錯誤、檔案路徑）。建議：非 Development 環境改回泛用訊息，細節只進 log（log 端已完整記錄，`:43-50`）。
