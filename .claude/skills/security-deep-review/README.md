# Security Deep Review

深度程式碼安全審查工具，專注於分析程式邏輯與業務安全問題。

## 📋 概述

**類型**：人工程式碼審查輔助  
**方法**：閱讀程式碼 + 邏輯分析  
**速度**：🐢 較慢（需詳細分析）  
**範圍**：🔍 深度優先（特定檔案/模組）

## 🎯 主要功能

### 1. 注入攻擊深度分析
- SQL Injection 參數化檢查
- Command Injection 輸入驗證
- 安全編碼模式驗證

### 2. XSS 防護檢查
- 使用者輸入處理流程
- 輸出編碼檢查
- 框架安全機制使用

### 3. 身份驗證問題
- 密碼雜湊演算法檢查
- JWT 安全配置
- Session 管理機制
- 密鑰強度驗證

### 4. 授權問題 (IDOR)
- 資源所有權驗證
- 權限檢查邏輯
- 橫向越權漏洞
- 垂直越權漏洞

### 5. CSRF 防護
- Token 驗證機制
- SameSite Cookie 配置
- 來源驗證檢查

### 6. 輸入驗證
- 驗證邏輯完整性
- 型別檢查
- 業務規則驗證
- 邊界條件檢查

## 🚀 使用方式

### 審查單一檔案
```bash
@workspace 使用 security-deep-review 審查 src/auth/login.ts
```

### 審查模組
```bash
@workspace 使用 security-deep-review 審查 src/auth/ 目錄下的所有檔案
```

### 聚焦特定問題
```bash
@workspace 使用 security-deep-review 審查支付模組的授權邏輯
```

## 📊 報告格式

```
🔒 程式碼安全審查報告
==========================================

[CRITICAL] SQL Injection
檔案: backend/api/users.go:78
程式碼:
  76 | func GetUser(id string) (*User, error) {
> 78 |     query := fmt.Sprintf("SELECT * FROM users WHERE id = %s", id)
  79 |     var user User

問題: 使用字串拼接建構 SQL 查詢，容易受到 SQL Injection 攻擊
風險: 攻擊者可以執行任意 SQL 指令
修復建議:
  query := "SELECT * FROM users WHERE id = $1"
  err := db.QueryRow(query, id).Scan(&user)

參考: https://owasp.org/www-community/attacks/SQL_Injection
```

## ⏰ 使用時機

### ✅ 適合使用
- ✅ Pull Request 程式碼審查
- ✅ 重構後驗證
- ✅ 實作安全關鍵功能（登入、支付、授權）
- ✅ 複雜業務邏輯檢查
- ✅ 漏洞修復驗證
- ✅ 安全培訓與學習

### ❌ 不適合使用
- ❌ 需要快速掃描整個專案
- ❌ 檢查第三方依賴套件漏洞
- ❌ 自動化 CI/CD 流程
- ❌ 定期例行檢查

## 🔗 相關 Skills

- **security-fast-scan** - 快速安全掃描
- **security-check-secrets** - 敏感資料檢查
- **security-check-config** - 安全配置檢查

## 💡 最佳實踐

### Pull Request 審查
```bash
# 審查 PR 中變更的檔案
@workspace 使用 security-deep-review 審查以下檔案：
- src/auth/login.ts
- src/auth/register.ts
- src/middleware/auth.ts
```

### 安全關鍵功能
```bash
# 實作支付功能後
@workspace 使用 security-deep-review 深度審查支付模組的安全性
```

### 組合使用
```bash
# 1. 快速掃描發現問題
@workspace 使用 security-fast-scan 掃描專案

# 2. 針對問題區域深度審查
@workspace 使用 security-deep-review 審查發現問題的檔案
```

## 📈 檢查重點

### Critical（立即修復）
- [ ] SQL Injection
- [ ] Command Injection
- [ ] 硬編碼的生產環境憑證
- [ ] Remote Code Execution

### High（7天內）
- [ ] XSS 漏洞
- [ ] Authentication bypass
- [ ] IDOR
- [ ] 不安全的密碼儲存

### Medium（30天內）
- [ ] CSRF 缺少防護
- [ ] 弱加密
- [ ] 缺少輸入驗證

### Low（下個版本）
- [ ] 詳細錯誤訊息
- [ ] 過時的依賴套件

## 📚 參考資源

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [CWE Top 25](https://cwe.mitre.org/top25/)
- [完整文件](./SKILL.md)
- [安全報告範本](../security-fast-scan/assets/security-report-template.md)
