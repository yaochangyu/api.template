# .github/skills 資料夾結構修正報告（正確版本）

## 📋 GitHub 官方規範

根據 [GitHub 官方文件](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills)：

> **Note**: Skill files must be named `SKILL.md`.

### ✅ 正確的 Skill 結構

```
.github/skills/
├── skill-name/
│   ├── SKILL.md          # ⚠️ 必須是大寫 SKILL.md
│   ├── assets/           # 可選：程式碼範本、腳本
│   └── references/       # 可選：參考文件
```

## ✅ 已修正的問題

### 1️⃣ 檔案結構標準化
根據 GitHub 官方文件，Skills 必須放在獨立的子資料夾中，以下檔案已修正：

- ✅ `check-config.md` → `check-config/SKILL.md`
- ✅ `check-dependencies.md` → `check-dependencies/SKILL.md`
- ✅ `check-secrets.md` → `check-secrets/SKILL.md`
- ✅ `review-security.md` → `review-security/SKILL.md`

### 2️⃣ YAML Frontmatter 標準化
根據 GitHub Copilot 規範，使用 `name:` 而非 `skill:`

所有 17 個 SKILL.md 檔案的 YAML frontmatter 已標準化：
```yaml
---
name: skill-name              # 必填：唯一識別名稱
description: 簡短描述          # 必填：說明功能與使用時機
license: MIT                  # 可選：授權條款
---
```

## 📊 當前結構狀態

### ✅ 符合規範的 Skills（17 個）

| # | Skill 名稱 | 路徑 | 檔案名稱 | YAML |
|---|-----------|------|---------|------|
| 1 | api-development | `api-development/` | ✅ SKILL.md | ✅ name: |
| 2 | bdd-practices | `bdd-practices/` | ✅ SKILL.md | ✅ name: |
| 3 | bdd-testing | `bdd-testing/` | ✅ SKILL.md | ✅ name: |
| 4 | check-config | `check-config/` | ✅ SKILL.md | ✅ name: |
| 5 | check-dependencies | `check-dependencies/` | ✅ SKILL.md | ✅ name: |
| 6 | check-secrets | `check-secrets/` | ✅ SKILL.md | ✅ name: |
| 7 | ef-core | `ef-core/` | ✅ SKILL.md | ✅ name: |
| 8 | error-handling | `error-handling/` | ✅ SKILL.md | ✅ name: |
| 9 | handler | `handler/` | ✅ SKILL.md | ✅ name: |
| 10 | middleware | `middleware/` | ✅ SKILL.md | ✅ name: |
| 11 | project-init | `project-init/` | ✅ SKILL.md | ✅ name: |
| 12 | repository-design | `repository-design/` | ✅ SKILL.md | ✅ name: |
| 13 | review-security | `review-security/` | ✅ SKILL.md | ✅ name: |
| 14 | security-scan | `security-scan/` | ✅ SKILL.md | ✅ name: |
| 15 | skill-agent-design | `skill-agent-design/` | ✅ SKILL.md | ✅ name: |
| 16 | skill-creator | `skill-creator/` | ✅ SKILL.md | ✅ name: |
| 17 | webapi-real-testing | `webapi-real-testing/` | ✅ SKILL.md | ✅ name: |

### 📁 特殊檔案/資料夾

- `security-checklist.md` - 安全檢查清單（參考文件，非 skill）
- `templates/` - 範本資料夾
  - `security-report-template.md` - 安全報告範本

這些檔案不是 skill 定義，是專案的參考資料。

## 📖 SKILL.md 檔案格式範例

```markdown
---
name: webapp-testing
description: Skill for testing web applications with Playwright
license: MIT
---

# Web Application Testing Skill

## 描述
此 skill 提供使用 Playwright 進行 Web 應用程式測試的指導。

## 職責
- 建立端到端測試腳本
- 執行瀏覽器自動化測試
- 產生測試報告

## 何時使用
當 Copilot 需要：
- 建立或修改 Web 測試時
- 自動化瀏覽器操作時
- 驗證 UI 功能時

## 指令與範例
[詳細說明...]
```

## 🎯 驗證結果

### 完全符合 GitHub 規範 ✅

- ✅ 所有 skills 都在獨立子資料夾中
- ✅ 所有主檔案都命名為 `SKILL.md`（大寫）
- ✅ 所有 YAML frontmatter 使用 `name:` 欄位
- ✅ 所有 skills 都包含 `description` 欄位
- ✅ 檔案結構清晰且一致

### 使用方式

根據 GitHub 文件，Copilot 會根據您的 prompt 和 skill 的 description 自動決定何時使用 skills：

```
# Copilot 會自動判斷並使用適當的 skill
建立新的 API 端點           → 可能使用 api-development skill
撰寫 Handler 業務邏輯      → 可能使用 handler skill
執行安全掃描               → 可能使用 security-scan skill
```

當 Copilot 選擇使用某個 skill 時，`SKILL.md` 檔案的內容會被注入到 agent 的 context 中，讓 agent 能夠遵循您定義的指令。

## ⚠️ 重要提醒

### 檔案命名規則

根據官方文件明確說明：

> **Skill files must be named `SKILL.md`.**

- ✅ **正確**：`SKILL.md`（全部大寫）
- ❌ **錯誤**：`skill.md`（小寫）
- ❌ **錯誤**：`Skill.md`（首字母大寫）

### 資料夾命名規則

> Skill directory names should be lowercase, use hyphens for spaces, and typically match the `name` in the `SKILL.md` frontmatter.

- ✅ **正確**：`webapp-testing/`
- ❌ **錯誤**：`WebApp-Testing/`
- ❌ **錯誤**：`webapp_testing/`

## 📚 參考資源

### 官方文件
- [About agent skills](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills)
- [Creating custom agents](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents)
- [Custom agents configuration](https://docs.github.com/en/copilot/reference/custom-agents-configuration)

### 專案 Skills 位置
- **專案層級**：`.github/skills/` 或 `.claude/skills/`
- **個人層級**：`~/.copilot/skills/` 或 `~/.claude/skills/`

## 🎉 總結

所有問題已修正完成！`.github/skills/` 資料夾現在完全符合 GitHub Copilot 的官方規範：

- ✅ 17 個 skills 都使用 `SKILL.md`（大寫）檔名
- ✅ 所有 skills 都在獨立的 kebab-case 命名資料夾中
- ✅ 所有 YAML frontmatter 使用正確的 `name:` 欄位
- ✅ 結構清晰，可供 Copilot 正確識別與使用
