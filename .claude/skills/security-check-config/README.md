# Security Check Config

安全配置檢查工具，專門檢查應用程式的安全配置問題。

## 📋 概述

**檢查範圍**：HTTPS、CORS、Security Headers、Cookie、環境配置  
**檢查方法**：配置檔掃描 + 模式匹配  
**適用專案**：Web 應用程式、API 服務

## 🎯 主要檢查項目

### 1. 網路安全
- ✅ HTTPS 強制重導向
- ✅ TLS 版本檢查（≥ 1.2）
- ✅ HSTS (HTTP Strict Transport Security)
- ✅ Certificate Pinning

### 2. HTTP Security Headers
- ✅ Content-Security-Policy
- ✅ X-Frame-Options
- ✅ X-Content-Type-Options
- ✅ Referrer-Policy
- ✅ Permissions-Policy

### 3. CORS 配置
- ✅ 允許來源白名單檢查
- ✅ 允許方法限制
- ✅ Credentials 配置
- ✅ 避免 `Access-Control-Allow-Origin: *`

### 4. Cookie 安全
- ✅ httpOnly 屬性
- ✅ secure 屬性
- ✅ sameSite 設定
- ✅ Cookie Prefix (`__Secure-`, `__Host-`)

### 5. 環境配置
- ✅ 生產環境關閉 Debug
- ✅ 錯誤訊息不洩漏資訊
- ✅ .env 檔案不在版本控制中

## 🚀 使用方式

### 全面檢查
```bash
@workspace 使用 security-check-config 檢查所有安全配置
```

### 指定檢查項目
```bash
@workspace 使用 security-check-config 檢查 CORS 配置
```

### 檢查特定環境
```bash
@workspace 使用 security-check-config 檢查生產環境配置
```

## 📊 檢查報告範例

```
⚙️ 安全配置檢查報告
==========================================
檢查時間: 2026-01-09 16:20:00
檢查環境: Production

❌ 發現的問題
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[HIGH-001] CORS 配置過於寬鬆
檔案: server.ts:45
問題: Access-Control-Allow-Origin 設定為 *
風險: 任何網站都可以發送跨域請求
建議: 設定明確的白名單
  allowedOrigins: ['https://example.com', 'https://app.example.com']

[MEDIUM-002] Cookie 缺少 httpOnly
檔案: auth.middleware.ts:23
問題: session cookie 沒有設定 httpOnly
風險: 可能被 XSS 攻擊竊取
建議: 
  res.cookie('session', token, {
    httpOnly: true,
    secure: true,
    sameSite: 'strict'
  });
```

## ⏰ 使用時機

### ✅ 適合使用
- ✅ 部署到新環境前
- ✅ 更改配置檔後
- ✅ 定期配置審查（每月）
- ✅ 發現配置問題時
- ✅ 安全稽核準備

### 🔄 建議頻率
- 配置變更後：立即檢查
- 定期檢查：每月一次
- 環境遷移：遷移前後各一次

## 🎯 檢查清單

### 網路安全
- [ ] HTTPS 強制
- [ ] TLS 1.2+
- [ ] HSTS 已啟用
- [ ] SSL/TLS 加密
- [ ] 連線池配置
- [ ] 連線超時設定

### HTTP Headers
- [ ] CSP 已設定
- [ ] X-Frame-Options 已設定
- [ ] X-Content-Type-Options 已設定
- [ ] Referrer-Policy 已設定
- [ ] Permissions-Policy 已設定

### CORS
- [ ] 明確的來源白名單
- [ ] 限制允許的方法
- [ ] Credentials 正確配置

### Cookie
- [ ] httpOnly 已啟用
- [ ] secure 已啟用
- [ ] sameSite 已設定

### 環境
- [ ] .env 不在 Git 中
- [ ] 生產環境關閉 Debug
- [ ] 錯誤訊息不洩漏資訊

## 🔗 相關 Skills

- **security-fast-scan** - 快速安全掃描
- **security-deep-review** - 程式碼安全深度審查
- **security-check-dependencies** - 依賴套件檢查

## 💡 常見問題修復

### CORS 配置
```javascript
// ❌ 錯誤
app.use(cors({ origin: '*' }));

// ✅ 正確
const allowedOrigins = ['https://example.com', 'https://app.example.com'];
app.use(cors({
  origin: (origin, callback) => {
    if (!origin || allowedOrigins.includes(origin)) {
      callback(null, true);
    } else {
      callback(new Error('Not allowed by CORS'));
    }
  },
  credentials: true
}));
```

### Cookie 安全
```javascript
// ❌ 錯誤
res.cookie('session', token);

// ✅ 正確
res.cookie('session', token, {
  httpOnly: true,
  secure: true,
  sameSite: 'strict',
  maxAge: 3600000
});
```

### Security Headers
```javascript
// ✅ 使用 helmet
const helmet = require('helmet');
app.use(helmet());
app.use(helmet.contentSecurityPolicy({
  directives: {
    defaultSrc: ["'self'"],
    styleSrc: ["'self'", "'unsafe-inline'"],
    scriptSrc: ["'self'"],
    imgSrc: ["'self'", "data:", "https:"]
  }
}));
```

## 📚 參考資源

- [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/)
- [Mozilla Observatory](https://observatory.mozilla.org/)
- [Security Headers](https://securityheaders.com/)
- [完整文件](./SKILL.md)
- [安全報告範本](../security-fast-scan/assets/security-report-template.md)
