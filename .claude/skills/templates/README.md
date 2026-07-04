# Security Report Template 使用指南

## 📋 範本說明

`security-report-template.md` 是一個全面的安全檢查報告範本，可被多個安全相關的 skills 參考使用。

### 範本位置
```
.github/skills/templates/security-report-template.md
```

### 範本內容涵蓋

1. **執行摘要**
   - 總體評分
   - 問題統計
   - 問題分類（餅圖）
   - 風險趨勢

2. **詳細發現**（按嚴重程度分類）
   - 🔴 Critical（嚴重）
   - 🟠 High（高）
   - 🟡 Medium（中）
   - 🔵 Low（低）
   - ⚪ Info（資訊）

3. **依賴套件漏洞**
   - 已知漏洞清單
   - 更新建議

4. **安全配置檢查**
   - 網路安全
   - HTTP Security Headers
   - CORS 配置
   - Cookie 安全

5. **合規性檢查**
   - OWASP Top 10 (2021)
   - CWE Top 25

6. **修復優先級建議**
   - 立即修復（Critical）
   - 高優先級（7天內）
   - 中優先級（30天內）
   - 低優先級（下個版本）

7. **安全最佳實踐建議**
   - 短期建議（1-3個月）
   - 中期建議（3-6個月）
   - 長期建議（6-12個月）

## 🔗 使用此範本的 Skills

### 1. security-fast-scan
**完整使用**：產生全面的安全掃描報告

```markdown
使用 security-fast-scan skill 時，產生的報告會參考完整的範本結構，包含：
- 所有章節（執行摘要、詳細發現、依賴套件、配置檢查、合規性）
- 完整的嚴重程度分類
- OWASP Top 10 檢查結果
```

**參考章節**：全部

### 2. security-deep-review
**部分使用**：產生程式碼安全審查報告

```markdown
使用 security-deep-review skill 時，主要參考：
- 詳細發現（Critical/High/Medium/Low）
- 問題描述格式（檔案位置、漏洞類型、程式碼片段、修復建議）
- 修復優先級建議
```

**參考章節**：
- 詳細發現
- 修復優先級建議
- 程式碼範例格式

### 3. security-check-config
**部分使用**：產生安全配置檢查報告

```markdown
使用 security-check-config skill 時，主要參考：
- 安全配置檢查章節
  - 網路安全
  - HTTP Security Headers
  - CORS 配置
  - Cookie 安全
```

**參考章節**：
- 安全配置檢查
- 問題統計表格
- 修復建議格式

### 4. security-check-dependencies
**部分使用**：產生依賴套件檢查報告

```markdown
使用 security-check-dependencies skill 時，主要參考：
- 依賴套件漏洞章節
  - 已知漏洞清單表格
  - 更新建議與命令
```

**參考章節**：
- 依賴套件漏洞
- 更新建議
- 問題統計

### 5. security-check-secrets
**部分使用**：產生敏感資料掃描報告

```markdown
使用 security-check-secrets skill 時，主要參考：
- 詳細發現中的敏感資料洩漏部分
- Critical 級別問題格式
- 修復建議（立即撤銷、移除、環境變數）
```

**參考章節**：
- 詳細發現（Critical 級別）
- 修復建議
- 問題描述格式

## 📊 範本使用對照表

| Skill | 使用範圍 | 主要參考章節 | 報告類型 |
|-------|---------|------------|---------|
| **security-fast-scan** | ✅ 完整使用 | 所有章節 | 全面安全掃描報告 |
| **security-deep-review** | 🔶 部分使用 | 詳細發現、修復優先級 | 程式碼安全審查報告 |
| **security-check-config** | 🔶 部分使用 | 安全配置檢查 | 配置安全檢查報告 |
| **security-check-dependencies** | 🔶 部分使用 | 依賴套件漏洞 | 依賴套件檢查報告 |
| **security-check-secrets** | 🔶 部分使用 | 敏感資料洩漏 | 敏感資料掃描報告 |

## 🎯 使用建議

### 1. 完整安全掃描
當需要全面的安全報告時：
```bash
@workspace 使用 security-fast-scan 執行完整安全掃描並產生詳細報告
```
→ 使用完整範本，涵蓋所有安全面向

### 2. 特定領域檢查
當只需要檢查特定安全面向時：

**程式碼安全**：
```bash
@workspace 使用 security-deep-review 審查程式碼安全問題
```
→ 參考範本的詳細發現章節

**配置安全**：
```bash
@workspace 使用 security-check-config 檢查安全配置
```
→ 參考範本的安全配置檢查章節

**依賴套件**：
```bash
@workspace 使用 security-check-dependencies 檢查依賴套件漏洞
```
→ 參考範本的依賴套件漏洞章節

**敏感資料**：
```bash
@workspace 使用 security-check-secrets 掃描敏感資料洩漏
```
→ 參考範本的敏感資料相關部分

### 3. 組合使用
執行多個專項檢查後，統整為完整報告：
```bash
# 步驟 1：執行各項檢查
@workspace 使用 security-check-secrets 掃描敏感資料
@workspace 使用 security-check-config 檢查配置
@workspace 使用 security-check-dependencies 檢查依賴套件
@workspace 使用 security-deep-review 審查程式碼

# 步驟 2：統整報告
請將以上檢查結果統整為一份完整的安全報告，參考 security-report-template.md 格式
```

## 📝 自訂報告

### 修改範本
如果需要自訂報告格式：

1. **複製範本**
   ```bash
   cp .github/skills/templates/security-report-template.md \
      .github/skills/templates/security-report-template-custom.md
   ```

2. **調整章節**
   - 新增/移除章節
   - 調整嚴重程度分類
   - 自訂問題格式

3. **更新 Skills 參考**
   在各個 skill 的 SKILL.md 中更新範本路徑

### 擴展範本
範本可以擴展以支援：
- 特定產業的合規性要求（GDPR、HIPAA、PCI-DSS）
- 特定技術棧的檢查項目
- 客製化的風險評估標準
- 自動化修復腳本

## 🔧 維護建議

### 定期更新
- 每季更新 OWASP Top 10 檢查項目
- 每半年更新 CWE Top 25 清單
- 依據新的安全威脅調整範本

### 版本控制
在範本中記錄版本資訊：
```markdown
### C. 變更歷史

| 版本 | 日期 | 變更內容 | 作者 |
|-----|------|---------|------|
| 1.0 | 2024-01-01 | 初始版本 | Team |
| 1.1 | 2024-06-01 | 新增 OWASP Top 10 2021 | Team |
```

## 📚 參考資源

### 安全標準
- [OWASP Top 10 (2021)](https://owasp.org/www-project-top-ten/)
- [CWE Top 25](https://cwe.mitre.org/top25/)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)

### 報告範例
- OWASP Testing Guide 報告範本
- SANS Penetration Testing 報告格式
- PCI DSS Compliance 報告要求

### 工具整合
範本可與以下工具的輸出整合：
- **SAST**：SonarQube, Semgrep, CodeQL
- **DAST**：OWASP ZAP, Burp Suite
- **依賴掃描**：Snyk, Dependabot, npm audit
- **密鑰掃描**：TruffleHog, GitLeaks

## 🎉 總結

`security-report-template.md` 提供了一個標準化、全面的安全報告框架，被 5 個安全相關 skills 使用：

1. **security-fast-scan**：完整使用，產生全面報告
2. **security-deep-review**：程式碼審查部分
3. **security-check-config**：配置檢查部分
4. **security-check-dependencies**：依賴套件部分
5. **security-check-secrets**：敏感資料部分

透過統一的範本，確保所有安全檢查產生的報告格式一致、內容完整、易於理解與追蹤。
