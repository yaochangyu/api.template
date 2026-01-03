# API 設計與安全性強化指引

本文件提供了 RESTful API 設計原則和 API 安全性防護措施。

## RESTful API 設計原則

#### API 版本控制策略
支援 URL 路徑版本控制與標頭版本控制。

#### 內容協商與媒體類型
- **Accept 標頭處理**: 支援多種回應格式 (JSON, XML)
- **內容壓縮**: 自動 Gzip/Brotli 壓縮
- **API 文件**: 整合 Swagger/OpenAPI 3.0 規格

## API 安全性防護

#### 輸入驗證與清理
使用 FluentValidation 或 DataAnnotations 進行模型驗證，防止 SQL Injection、XSS 等攻擊。

#### CORS 與跨來源安全
根據環境設定不同的 CORS 政策，生產環境限制允許的來源。

#### HTTPS 強制與安全標頭
- HTTPS 重新導向與 HSTS
- 安全標頭：X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, CSP

#### API 限流與頻率控制
使用 AspNetCoreRateLimit 套件實作限流機制，防止 DDoS 攻擊。

#### 機敏設定管理
**核心原則**: 不要在 `appsettings.json` 儲存機密。

- ❌ **禁止**: 在 `appsettings.json` 放入連線字串、金鑰、權杖
- ✅ **改用**: 環境變數、.NET User Secrets（本機）、Docker Secrets（容器）、雲端祕密管家
