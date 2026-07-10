---
name: security-check-secrets
description: 掃描程式碼和配置檔中的敏感資料洩漏，包括 API Keys、密碼、Token 等
---

### ⚠️ 前置條件
本 SKILL 須搭配閱讀：[開發規則](../../development-rules.md)

# Security Check Secrets

專門掃描敏感資料洩漏，防止憑證被提交到版本控制系統。

## 何時使用

- Git commit 之前
- 推送到遠端儲存庫之前
- 發現可疑的配置檔時
- 定期安全審查

## 執行步驟

### 1. 掃描雲端服務 API Keys

使用 Grep 工具搜尋以下模式：

**AWS Keys:**
```
pattern: "AKIA[0-9A-Z]{16}|ASIA[0-9A-Z]{16}"
files: "**/*.{ts,js,go,py,java,env,json,yaml,yml}"
```

**OpenAI API Keys:**
```
pattern: "sk-[a-zA-Z0-9]{48}|sk-proj-[a-zA-Z0-9]{48}"
files: "**/*.{ts,js,go,py,env}"
```

**Google Cloud:**
```
pattern: "AIza[0-9A-Za-z\\-_]{35}"
files: "**/*.{ts,js,py,env,json}"
```

**Stripe:**
```
pattern: "sk_live_[0-9a-zA-Z]{24,}|sk_test_[0-9a-zA-Z]{24,}"
files: "**/*.{ts,js,env}"
```

**GitHub Tokens:**
```
pattern: "ghp_[0-9a-zA-Z]{36}|gho_[0-9a-zA-Z]{36}"
files: "**/*.{ts,js,go,py,env,yaml,yml}"
```

### 2. 掃描資料庫憑證

```
pattern: "postgresql://[^:]+:[^@]+@|mysql://[^:]+:[^@]+@|mongodb(\\+srv)?://[^:]+:[^@]+@"
files: "**/*.{ts,js,go,py,env,yaml,yml}"
```

### 3. 掃描 JWT Secrets

```
pattern: "jwt[_-]?secret\\s*[:=]\\s*[\"'][^\"']{16,}[\"']|JWT_SECRET\\s*=\\s*[\"'][^\"']{16,}[\"']"
files: "**/*.{ts,js,go,py,env}"
```

### 4. 掃描 SSH 和 SSL 私鑰

```
pattern: "-----BEGIN (RSA |DSA |EC |OPENSSH )?PRIVATE KEY-----"
files: "**/*.{pem,key,txt}"
```

### 5. 掃描硬編碼密碼

```
pattern: "password\\s*[:=]\\s*[\"'][^\"']{6,}[\"']|passwd\\s*[:=]\\s*[\"'][^\"']{6,}[\"']"
files: "**/*.{ts,js,go,py,java,cs}"
```

### 6. 檢查 .env 檔案

檢查以下檔案是否被 .gitignore 排除：
- `.env`
- `.env.local`
- `.env.production`
- `config/secrets.json`
- `credentials.json`

### 7. 產生報告

對於每個發現的敏感資料，回報：

```
🔍 敏感資料掃描報告
==========================================

[CRITICAL] AWS Access Key
位置: backend/config/aws.go:12
內容: AKIA****[已遮罩]
風險: 完全存取 AWS 資源
修復:
  1. 立即在 AWS Console 撤銷此 Access Key
  2. 使用環境變數或 AWS Secrets Manager
  3. 從 Git 歷史中移除
```

## 參數

- `target_path` (選填): 掃描目錄，預設為當前目錄
- `--include-git-history`: 掃描 Git 歷史記錄
- `--entropy-check`: 啟用高熵值字串檢測（偵測可能的密鑰）

## 排除清單

預設排除以下目錄：
- `node_modules/`
- `vendor/`
- `dist/`
- `build/`
- `.git/objects/`

## 誤報處理

對於確認是誤報的項目，可以建立白名單：`.claude/secrets-whitelist.json`

## 修復建議

發現敏感資料後的處理步驟：
1. **立即撤銷**: 在對應服務中撤銷洩漏的憑證
2. **移除**: 從程式碼中移除硬編碼的憑證
3. **環境變數**: 使用環境變數或密鑰管理服務
4. **Git 歷史**: 使用 git-filter-repo 從歷史中移除
5. **通知團隊**: 告知安全團隊和相關人員

## 參考資料

- [OWASP: Use of Hard-coded Password](https://owasp.org/www-community/vulnerabilities/Use_of_hard-coded_password)
- [GitHub: Removing sensitive data](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/removing-sensitive-data-from-a-repository)

## 報告範本

產生敏感資料掃描報告時，可參考：
- `../security-fast-scan/assets/security-report-template.md` - 完整報告範本（參考「敏感資料洩漏」相關章節）

## 相關 Skills
- `security-fast-scan` - 快速安全掃描
- `security-deep-review` - 程式碼安全深度審查
- `security-check-config` - 檢查配置中的硬編碼憑證
