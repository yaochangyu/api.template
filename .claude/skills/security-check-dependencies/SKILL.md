---
name: security-check-dependencies
description: 檢查專案依賴套件的已知漏洞、過時版本和安全問題
---

# Security Check Dependencies

檢查專案依賴套件的已知漏洞和安全問題。

## 何時使用

- 安裝新套件後
- 定期安全更新（每週）
- CI/CD pipeline 中
- 發布前的最後檢查

## 執行步驟

### 1. 偵測套件管理工具

使用 Glob 工具尋找套件管理檔案：

```
- package.json (Node.js)
- go.mod (Go)
- requirements.txt (Python)
- pom.xml (Java Maven)
- build.gradle (Java Gradle)
- *.csproj (C# .NET)
- Gemfile (Ruby)
- composer.json (PHP)
```

### 2. 執行依賴檢查

根據偵測到的工具，執行對應的檢查命令：

**Node.js (npm/pnpm/yarn):**
```bash
cd [專案目錄]
npm audit --json
# 或
pnpm audit --json
# 或
yarn audit --json
```

**Go:**
```bash
cd [專案目錄]
go install golang.org/x/vuln/cmd/govulncheck@latest
govulncheck ./...
```

**Python:**
```bash
pip install safety
safety check --json
# 或
pip-audit --format json
```

**Java (Maven):**
```bash
mvn org.owasp:dependency-check-maven:check
```

**Java (Gradle):**
```bash
./gradlew dependencyCheckAnalyze
```

**C# (.NET):**
```bash
dotnet list package --vulnerable
```

**Ruby:**
```bash
bundle audit check --update
```

**PHP:**
```bash
composer audit --format=json
```

### 3. 解析檢查結果

從命令輸出中提取：
- 套件名稱
- 目前版本
- CVE 編號
- 嚴重程度 (Critical, High, Medium, Low)
- 可用的修復版本
- 漏洞描述

### 4. 產生報告

```
📦 依賴套件安全檢查報告
==========================================
檢查時間: [時間]
檢查目標: [路徑]

偵測到的套件管理工具:
  - Node.js (pnpm) - frontend/package.json
  - Go modules - backend/go.mod

🚨 發現 15 個已知漏洞
==========================================

frontend/package.json (pnpm)
------------------------------------------

[CRITICAL] Prototype Pollution in lodash
套件: lodash
版本: 4.17.15 (已安裝)
CVE: CVE-2020-8203
CVSS: 9.8

問題描述:
  lodash 4.17.15 之前的版本存在原型污染漏洞

影響範圍: lodash < 4.17.19
修復版本: >= 4.17.19
建議操作:
  pnpm update lodash@^4.17.19

backend/go.mod
------------------------------------------

[HIGH] HTTP/2 Rapid Reset Attack
套件: golang.org/x/net
版本: v0.0.0-20230425
CVE: CVE-2023-44487

影響範圍: < v0.17.0
修復版本: >= v0.17.0
建議操作:
  go get -u golang.org/x/net@latest
  go mod tidy

📊 統計
==========================================
依嚴重程度:
  🔴 Critical: 3
  🟠 High: 6
  🟡 Medium: 5
  🔵 Low: 1

🔧 快速修復指令
==========================================
# Frontend (pnpm)
cd frontend
pnpm update lodash@^4.17.19

# Backend (Go)
cd backend
go get -u golang.org/x/net@latest
go mod tidy
```

### 5. 提供更新建議

對於每個漏洞，提供具體的更新指令。

## 嚴重程度分級

- **Critical (9.0-10.0)**: 可遠端執行程式碼，完全系統控制
- **High (7.0-8.9)**: 可能導致資料洩漏或系統入侵
- **Medium (4.0-6.9)**: 有安全風險但需要特定條件
- **Low (0.1-3.9)**: 安全影響有限

## 參數

- `target_path` (選填): 檢查目錄，預設為當前目錄
- `--severity`: 最低嚴重程度 (critical, high, medium, low)
- `--update-suggestions`: 顯示更新建議
- `--audit-prod-only`: 只檢查生產環境依賴

## 自動更新建議

建議使用 Dependabot 或 Renovate 進行自動更新：

**.github/dependabot.yml:**
```yaml
version: 2
updates:
  - package-ecosystem: "npm"
    directory: "/frontend"
    schedule:
      interval: "weekly"
```

## 參考資料

- [OWASP Dependency-Check](https://owasp.org/www-project-dependency-check/)
- [GitHub Advisory Database](https://github.com/advisories)
- [Snyk Vulnerability Database](https://security.snyk.io/)

## 報告範本

產生依賴套件檢查報告時，可參考：
- `../security-fast-scan/assets/security-report-template.md` - 完整報告範本（參考「依賴套件漏洞」章節）

## 相關 Skills
- `security-fast-scan` - 快速安全掃描
- `security-check-config` - 安全配置檢查
- `security-deep-review` - 程式碼安全深度審查
