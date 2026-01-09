---
name: security-check-config
description: 檢查應用程式的安全配置，包括 HTTPS、CORS、Security Headers、Cookie 等
---

# Security Check Config

檢查應用程式的安全配置問題。

## 何時使用

- 部署到新環境前
- 更改配置檔後
- 定期配置審查
- 發現配置問題時

## 執行步驟

### 1. 尋找配置檔

使用 Glob 工具找出配置檔：

```
- next.config.js (Next.js)
- middleware.ts (Next.js)
- nginx.conf (Nginx)
- apache2.conf (Apache)
- main.go (Go Echo/Gin)
- *.env* (環境變數)
- config/*.{json,yaml,yml}
```

### 2. 檢查 HTTPS / TLS 配置

使用 Read 工具讀取 Nginx/Apache 配置：

❌ 不安全:
```nginx
ssl_protocols TLSv1 TLSv1.1 TLSv1.2;
```

✅ 安全:
```nginx
ssl_protocols TLSv1.2 TLSv1.3;
ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256';
add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
```

### 3. 檢查 Security Headers

讀取 Next.js 配置 (next.config.js)：

使用 Grep 搜尋:
- `Content-Security-Policy`
- `X-Frame-Options`
- `X-Content-Type-Options`
- `Strict-Transport-Security`

缺少的 headers 應該回報。

### 4. 檢查 CORS 配置

在 middleware.ts 或後端配置中搜尋：

❌ 危險:
```typescript
Access-Control-Allow-Origin: "*"
credentials: true
```

✅ 安全:
```typescript
const allowedOrigins = [
  'https://yourdomain.com',
  'https://app.yourdomain.com'
];
```

使用 Grep 搜尋: `Access-Control-Allow-Origin.*\*`

### 5. 檢查 Cookie 配置

搜尋 Cookie 設定：

使用 Grep 搜尋:
- `httpOnly.*false` (應該為 true)
- `secure.*false` (生產環境應該為 true)
- `sameSite` (應該設定為 strict 或 lax)

### 6. 檢查資料庫連線

搜尋資料庫連線配置：

使用 Grep 搜尋:
- `sslmode=disable` (應該為 require)
- `ssl.*false` (應該為 true)

### 7. 檢查環境變數配置

檢查 .gitignore 是否包含:
```
.env
.env.local
.env.production
config/secrets.json
```

### 8. 檢查 Debug Mode

使用 Grep 搜尋:
- `DEBUG.*true`
- `NODE_ENV.*development` (在生產配置中)
- `gin.SetMode(gin.DebugMode)` (在生產程式碼中)

### 9. 產生報告

```
⚙️ 安全配置檢查報告
==========================================
檢查時間: [時間]
檢查目標: [路徑]

🚨 發現 8 個配置問題
==========================================

[HIGH] 缺少 HSTS Header
位置: frontend/next.config.js
問題: 生產環境未設定 Strict-Transport-Security header
風險: 使用者可能透過不安全的 HTTP 連線存取網站
修復建議:
  async headers() {
    return [{
      source: '/:path*',
      headers: [{
        key: 'Strict-Transport-Security',
        value: 'max-age=63072000; includeSubDomains; preload'
      }]
    }]
  }

[CRITICAL] 不安全的 CORS 配置
位置: backend/middleware/cors.go:25
問題: 允許所有來源 (origin: "*") 且啟用憑證
風險: 任何網站都可以發送帶有憑證的請求到 API
修復建議:
  AllowOrigins: []string{
      "https://yourdomain.com",
  },

[HIGH] Cookie 缺少安全屬性
位置: backend/auth/session.go:42
問題: Session cookie 未設定 httpOnly, secure, sameSite
修復建議:
  http.SetCookie(w, &http.Cookie{
      Name:     "__Host-session",
      Value:    sessionID,
      HttpOnly: true,
      Secure:   true,
      SameSite: http.SameSiteStrictMode,
  })
```

## 檢查項目清單

### 網路安全
- [ ] HTTPS 強制
- [ ] TLS 1.2+ only
- [ ] HSTS header
- [ ] 強加密套件

### HTTP Headers
- [ ] Content-Security-Policy
- [ ] X-Frame-Options
- [ ] X-Content-Type-Options
- [ ] Referrer-Policy

### CORS
- [ ] 白名單來源
- [ ] 限制 HTTP 方法
- [ ] Credentials 配置正確

### Cookie
- [ ] httpOnly
- [ ] secure (生產環境)
- [ ] sameSite
- [ ] Cookie Prefix

### 資料庫
- [ ] SSL/TLS 加密
- [ ] 連線池配置
- [ ] 連線超時設定

### 環境
- [ ] .env 不在 Git 中
- [ ] 生產環境關閉 Debug
- [ ] 錯誤訊息不洩漏資訊

## 參數

- `target_path` (選填): 檢查目錄，預設為當前目錄
- `--focus`: 專注類型 (network, headers, cors, cookies, database)
- `--env`: 檢查環境 (development, production)

## 參考資料

- [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/)
- [Mozilla Observatory](https://observatory.mozilla.org/)
- [Security Headers](https://securityheaders.com/)

## 報告範本

產生配置檢查報告時，可參考：
- `../templates/security-report-template.md` - 完整報告範本（參考「安全配置檢查」章節）

## 相關 Skills
- `security-fast-scan` - 快速安全掃描
- `security-deep-review` - 程式碼安全深度審查
- `security-check-dependencies` - 依賴套件檢查
