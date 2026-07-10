---
name: security-deep-review
description: 深度程式碼安全審查，檢測注入攻擊、XSS、CSRF、身份驗證和授權問題
---

### ⚠️ 前置條件
本 SKILL 須搭配閱讀：[開發規則](../../DEVELOPMENT-RULES.md)

# Security Deep Review

深度程式碼層級的安全審查，專注於程式設計上的漏洞。

## 何時使用

- 程式碼審查時
- 重構大型功能後
- 實作安全關鍵功能時
- 定期程式碼品質檢查

## 執行步驟

### 1. 讀取目標檔案

使用 Read 工具讀取要審查的程式碼檔案。如果使用者提供目錄，使用 Glob 找出所有程式碼檔案。

### 2. 檢查注入攻擊漏洞

針對每個檔案，檢查以下模式：

**SQL Injection (TypeScript/JavaScript):**
- ❌ 字串拼接: `` `SELECT * FROM users WHERE id = ${userId}` ``
- ❌ 字串拼接: `"SELECT * FROM users WHERE id = " + userId`
- ✅ 正確: `db.query("SELECT * FROM users WHERE id = $1", [userId])`

**SQL Injection (Go):**
- ❌ 格式化: `fmt.Sprintf("SELECT * FROM users WHERE id = %s", userID)`
- ✅ 正確: `db.Query("SELECT * FROM users WHERE id = $1", userID)`

**Command Injection:**
- ❌ 危險: `exec(\`ping ${userInput}\`)`
- ❌ 危險: `eval(userCode)`
- ✅ 正確: `execFile("ping", ["-c", "4", validatedHost])`

### 3. 檢查 XSS 漏洞

**React/Next.js:**
- ❌ `<div dangerouslySetInnerHTML={{ __html: userInput }} />`
- ❌ `element.innerHTML = userInput`
- ✅ `<div>{userInput}</div>` (React 自動轉義)

**Go (html/template):**
- 確認使用 `html/template` 而非 `text/template`

### 4. 檢查身份驗證問題

**密碼處理:**
- ❌ 明文儲存: `password: req.body.password`
- ❌ 弱雜湊: `crypto.createHash("md5").update(password)`
- ✅ 正確: `bcrypt.hash(password, 12)`

**JWT 安全:**
- ❌ 弱密鑰: `jwt.sign({ userId }, "secret")`
- ❌ 無過期時間: `jwt.sign({ userId }, secret)`
- ✅ 正確: `jwt.sign({ userId }, process.env.JWT_SECRET, { algorithm: "HS256", expiresIn: "1h" })`

### 5. 檢查授權問題 (IDOR)

檢查是否驗證資源所有權：

❌ 不安全:
```typescript
app.delete("/api/posts/:id", async (req, res) => {
  await db.posts.delete({ id: req.params.id });
});
```

✅ 安全:
```typescript
app.delete("/api/posts/:id", async (req, res) => {
  const post = await db.posts.findUnique({ where: { id: req.params.id } });
  if (post.authorId !== req.user.id) {
    return res.status(403).send("Forbidden");
  }
  await db.posts.delete({ id: req.params.id });
});
```

### 6. 檢查加密問題

**弱演算法:**
- ❌ `crypto.createCipher("des", key)` - DES 已廢棄
- ❌ `crypto.createHash("md5")` - MD5 不安全
- ✅ `crypto.createCipheriv("aes-256-gcm", key, iv)`

**不安全的亂數:**
- ❌ `Math.random()` - 不安全
- ✅ `crypto.randomBytes(32)`

### 7. 檢查輸入驗證

確認所有使用者輸入都經過驗證：

❌ 未驗證:
```typescript
const age = req.body.age;
```

✅ 已驗證:
```typescript
const schema = z.object({
  age: z.number().int().min(0).max(150)
});
const validated = schema.parse(req.body);
```

### 8. 產生審查報告

對於每個發現的問題，提供：

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

## 檢查重點

依嚴重程度分類：

**Critical (立即修復):**
- SQL Injection
- Command Injection
- 硬編碼的生產環境憑證
- Remote Code Execution

**High (7 天內):**
- XSS 漏洞
- Authentication bypass
- IDOR
- 敏感資料洩漏

**Medium (30 天內):**
- 缺少 CSRF 保護
- 弱加密
- 缺少輸入驗證

**Low (下個版本):**
- 詳細錯誤訊息
- 過時的依賴套件

## 參數

- `target_path` (選填): 要審查的檔案或目錄
- `--language`: 指定語言 (typescript, go, python, java)
- `--focus`: 專注類型 (injection, xss, csrf, auth)
- `--severity`: 最低嚴重程度 (critical, high, medium, low)

## 參考資料

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [CWE Top 25](https://cwe.mitre.org/top25/)

## 報告範本

產生詳細安全審查報告時，請參考：
- `../security-fast-scan/assets/security-report-template.md` - 完整的安全檢查報告範本

## 相關 Skills
- `security-fast-scan` - 完整安全掃描
- `security-check-secrets` - 敏感資料檢查
- `security-check-config` - 安全配置檢查
