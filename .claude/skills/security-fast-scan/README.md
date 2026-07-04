# Security Fast Scan

快速安全掃描工具，整合多種檢查項目，提供自動化的全面安全掃描。

## 📋 概述

**類型**：綜合型自動化掃描  
**方法**：正規表達式模式匹配 + 工具整合  
**速度**：⚡ 快速（數分鐘）  
**範圍**：📊 廣度優先（整個專案）

## 🎯 主要功能

### 1. 注入攻擊掃描
- SQL Injection 模式檢測
- Command Injection 檢測
- 字串拼接安全檢查

### 2. XSS 漏洞掃描
- dangerouslySetInnerHTML 檢測
- innerHTML 使用檢查
- document.write 檢測

### 3. 敏感資料掃描
- AWS Access Key 模式
- OpenAI API Key 模式
- 硬編碼密碼檢測
- API Key 洩漏檢查

### 4. 依賴套件漏洞
- npm audit (Node.js)
- govulncheck (Go)
- dotnet list package (.NET)

### 5. 安全配置檢查
- CORS 配置檢查
- Cookie 安全屬性
- Debug mode 檢測

## 🚀 使用方式

### 基本用法
```bash
@workspace 使用 security-fast-scan 掃描整個專案
```

### 指定路徑
```bash
@workspace 使用 security-fast-scan 掃描 src/WebAPI 目錄
```

## 📊 報告格式

```
🔒 安全掃描報告
==========================================
掃描時間: 2026-01-09 16:20:00
掃描目標: /project/src

📊 統計資訊
🔴 嚴重 (Critical): 2
🟠 高 (High): 5
🟡 中 (Medium): 10
🔵 低 (Low): 3

⚠️ 發現的漏洞
[詳細列表]

✅ 建議修復步驟
[具體建議]
```

## ⏰ 使用時機

### ✅ 適合使用
- ✅ 定期安全掃描（每週/每月）
- ✅ CI/CD 整合
- ✅ Pull Request 提交前
- ✅ 新功能開發完成後
- ✅ 重大版本發布前
- ✅ 快速安全評估

### ❌ 不適合使用
- ❌ 深入分析業務邏輯漏洞
- ❌ 需要理解複雜的程式流程
- ❌ 檢查細微的授權問題

## 🔗 相關 Skills

- **security-deep-review** - 深度程式碼審查
- **security-check-config** - 安全配置檢查
- **security-check-dependencies** - 依賴套件檢查
- **security-check-secrets** - 敏感資料掃描

## 💡 最佳實踐

### 日常使用
```bash
# 每日/每次 PR
@workspace 使用 security-fast-scan 掃描專案
```

### 組合使用
```bash
# 1. 快速掃描
@workspace 使用 security-fast-scan 掃描專案

# 2. 發現問題後深度審查
@workspace 使用 security-deep-review 審查有問題的檔案
```

## 📈 成功指標

執行完成後檢查：
- [ ] 無 Critical 級別漏洞
- [ ] High 級別漏洞已追蹤
- [ ] 所有依賴套件已更新到安全版本
- [ ] 無硬編碼的敏感資料
- [ ] 安全配置符合最佳實踐

## 📚 參考資源

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [CWE Top 25](https://cwe.mitre.org/top25/)
- [完整文件](./SKILL.md)
- [安全報告範本](../templates/security-report-template.md)
