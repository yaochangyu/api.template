# Security Check Secrets

敏感資料掃描工具，防止 API Keys、密碼、Token 等憑證洩漏。

## 📋 概述

**檢查範圍**：程式碼、配置檔、環境變數、Git 歷史  
**檢查方法**：正規表達式模式匹配 + 熵值分析  
**嚴重程度**：Critical（立即處理）

## 🎯 檢測的敏感資料類型

### 1. Cloud Provider Credentials
- ✅ AWS Access Key (AKIA...)
- ✅ AWS Secret Key
- ✅ Azure Connection String
- ✅ Google Cloud API Key
- ✅ Alibaba Cloud AccessKey

### 2. API Keys
- ✅ OpenAI API Key (sk-...)
- ✅ Stripe API Key
- ✅ SendGrid API Key
- ✅ Twilio Auth Token
- ✅ GitHub Personal Access Token

### 3. Database Credentials
- ✅ PostgreSQL 連線字串
- ✅ MySQL 連線字串
- ✅ MongoDB 連線字串
- ✅ Redis 連線字串
- ✅ 硬編碼的資料庫密碼

### 4. Private Keys
- ✅ RSA Private Key
- ✅ SSH Private Key
- ✅ PGP Private Key
- ✅ JWT Secret

### 5. Generic Secrets
- ✅ 硬編碼密碼
- ✅ API Token
- ✅ Bearer Token
- ✅ Session Secret

## 🚀 使用方式

### 全專案掃描
```bash
@workspace 使用 security-check-secrets 掃描整個專案
```

### 指定目錄
```bash
@workspace 使用 security-check-secrets 掃描 src/config 目錄
```

### 提交前檢查
```bash
@workspace 使用 security-check-secrets 檢查即將提交的檔案
```

## 📊 報告格式

```
🔍 敏感資料掃描報告
==========================================
掃描時間: 2026-01-09 16:20:00
掃描範圍: 全專案

📊 統計資訊
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
掃描檔案: 1,234
發現問題: 5
🔴 Critical: 5

⚠️ 發現的敏感資料
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔴 [CRITICAL-001] AWS Access Key 洩漏
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
檔案: src/config/aws.ts
行號: 23
類型: AWS Access Key

程式碼:
  21 | export const awsConfig = {
  22 |   region: 'us-east-1',
> 23 |   accessKeyId: 'AKIAIOSFODNN7EXAMPLE',
  24 |   secretAccessKey: 'wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY'
  25 | };

風險等級: Critical
風險說明: AWS 憑證洩漏可能導致雲端資源被完全控制

✅ 立即處理步驟:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
1. 立即撤銷此 Access Key（AWS Console > IAM）
2. 從程式碼移除硬編碼憑證
3. 使用環境變數替代:
   export AWS_ACCESS_KEY_ID="..."
   export AWS_SECRET_ACCESS_KEY="..."
4. 使用 git-filter-repo 清除 Git 歷史
5. 通知安全團隊

參考: https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_access-keys.html
```

## ⏰ 使用時機

### ✅ 適合使用
- ✅ **每次提交前**（最重要！）
- ✅ Pull Request 審查
- ✅ 定期安全掃描（每週）
- ✅ 新成員加入專案後
- ✅ 重大版本發布前
- ✅ 安全稽核準備

### 🚨 緊急情況
- 🚨 發現洩漏後立即掃描 Git 歷史
- 🚨 公開儲存庫轉私有前
- 🚨 收到安全警告時

## 🎯 檢測模式範例

### AWS Access Key
```regex
AKIA[0-9A-Z]{16}
```

### OpenAI API Key
```regex
sk-[a-zA-Z0-9]{48}
```

### Generic API Key
```regex
api[_-]?key\s*[:=]\s*["'][^"']+["']
```

### Hardcoded Password
```regex
password\s*[:=]\s*["'][^"']{6,}["']
```

## 🔧 修復指南

### 步驟 1：立即撤銷
```bash
# AWS
aws iam delete-access-key --access-key-id AKIAIOSFODNN7EXAMPLE

# GitHub
# 前往 Settings > Developer settings > Personal access tokens > Delete

# 其他服務
# 登入對應服務的控制台撤銷
```

### 步驟 2：移除程式碼
```typescript
// ❌ 錯誤：硬編碼
const apiKey = "sk-abc123...";

// ✅ 正確：使用環境變數
const apiKey = process.env.OPENAI_API_KEY;
```

### 步驟 3：使用環境變數
```bash
# .env（不要提交到 Git）
OPENAI_API_KEY=sk-abc123...
AWS_ACCESS_KEY_ID=AKIA...
AWS_SECRET_ACCESS_KEY=...

# .env.example（可以提交）
OPENAI_API_KEY=your_openai_api_key_here
AWS_ACCESS_KEY_ID=your_aws_access_key_id
```

### 步驟 4：清除 Git 歷史
```bash
# 使用 git-filter-repo
git filter-repo --path src/config/secrets.ts --invert-paths

# 或使用 BFG Repo-Cleaner
bfg --delete-files secrets.ts
```

### 步驟 5：防止再次發生
```bash
# 設定 pre-commit hook
# .git/hooks/pre-commit
#!/bin/sh
npx secretlint "**/*"
```

## 🛡️ 預防措施

### 1. .gitignore 配置
```gitignore
# 敏感檔案
.env
.env.local
.env.*.local
secrets.json
credentials.json

# 私鑰
*.pem
*.key
*.p12
*.pfx
```

### 2. 環境變數管理
```javascript
// ✅ 使用 dotenv
require('dotenv').config();

// ✅ 驗證必要的環境變數
const requiredEnvVars = ['DATABASE_URL', 'API_KEY', 'JWT_SECRET'];
requiredEnvVars.forEach(varName => {
  if (!process.env[varName]) {
    throw new Error(`Missing required environment variable: ${varName}`);
  }
});
```

### 3. Secret Manager
```typescript
// ✅ 使用 AWS Secrets Manager
import { SecretsManager } from 'aws-sdk';
const secretsManager = new SecretsManager();

const secret = await secretsManager.getSecretValue({
  SecretId: 'prod/api/keys'
}).promise();

const apiKey = JSON.parse(secret.SecretString).OPENAI_API_KEY;
```

## 🔍 排除誤報

### 建立白名單
```json
// .secretlintrc.json
{
  "rules": [
    {
      "id": "example-pattern",
      "allowMessageIds": [
        "Example API key in documentation"
      ],
      "allow": [
        "/test/fixtures/example-secrets.js",
        "/docs/examples/**"
      ]
    }
  ]
}
```

## 🔗 相關 Skills

- **security-fast-scan** - 快速安全掃描（包含敏感資料掃描）
- **security-deep-review** - 程式碼安全深度審查
- **security-check-config** - 檢查配置中的硬編碼憑證

## 💡 最佳實踐

### 提交前檢查
```bash
# 每次提交前執行
@workspace 使用 security-check-secrets 檢查即將提交的變更
```

### 定期掃描
```bash
# 每週執行完整掃描
@workspace 使用 security-check-secrets 掃描整個專案包含 Git 歷史
```

### CI/CD 整合
```yaml
# .github/workflows/secrets-check.yml
name: Secrets Check
on: [push, pull_request]

jobs:
  check-secrets:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
        with:
          fetch-depth: 0  # 檢查完整歷史
      - uses: trufflesecurity/trufflehog@main
        with:
          path: ./
```

## 📈 成功指標

- [ ] 無硬編碼的憑證
- [ ] 所有敏感配置使用環境變數
- [ ] .env 檔案已加入 .gitignore
- [ ] Git 歷史中無敏感資料
- [ ] 已設定 pre-commit hook

## 📚 參考資源

- [OWASP: Use of Hard-coded Password](https://owasp.org/www-community/vulnerabilities/Use_of_hard-coded_password)
- [GitHub: Removing sensitive data](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/removing-sensitive-data-from-a-repository)
- [TruffleHog](https://github.com/trufflesecurity/trufflehog)
- [GitLeaks](https://github.com/gitleaks/gitleaks)
- [完整文件](./SKILL.md)
- [安全報告範本](../templates/security-report-template.md)
