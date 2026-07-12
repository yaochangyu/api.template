⚠️ **2026-07-12 註記**：本報告描述之分支 `health-check-fixes` 已於 4d5572a 併入 main 並刪除，內容為 2026-07-04 健檢輪的歷史紀錄。

---

# decisions.md — 健檢關鍵決策

日期：2026-07-04
性質：本次為「健檢診斷」，以下是修復方向的架構決策；實際修復依 plan.md 執行，未動任何既有檔案。

---

## D1. 文件單一真相來源（SSOT）：以根層 CLAUDE.md 為權威
- **決策**：CLAUDE.md 是唯一的 AI 助理規範權威。CLAUDE_OLD.md 回填必要獨有章節後刪除；`best-practices.md`（1786 行）與 `最佳實踐.md`（1311 行）合併為一份並從 CLAUDE.md 連結。
- **為什麼**：同一套規範目前散落 4 份（CLAUDE.md、CLAUDE_OLD.md、best-practices.md、最佳實踐.md），已出現「文件承諾的指令不存在」級別的漂移；多真相來源是本次健檢一半以上發現的根因。
- **捨棄的替代方案**：
  - 保留 CLAUDE_OLD.md 當歷史參考 → 棄：git 歷史已保存舊版，工作區留 128K 孤兒檔只會誤導 AI 與人。
  - 直接刪 CLAUDE_OLD.md 不回填 → 棄：它有 CLAUDE.md 缺的獨有內容（EF migration 指令範例 :968-983、部署 YAML :3146-3517、日誌與安全指引 :2534）。

## D2. 路徑修正方向：改文件、不改目錄名
- **決策**：把 CLAUDE.md 的 42 筆 `project-template/` 失效連結改成 `dotnet-project-template/` 前綴；目錄不改名。
- **為什麼**：目錄名 `dotnet-project-template` 已被 tree.md、近期 commit、.mcp.json 等多處使用；改文件是最小爆炸半徑，且 scan-docs 已驗證「補前綴後 42 筆全部 EXISTS」。
- **捨棄**：把目錄改回 `project-template/` → 棄：影響面更大、且新名稱語意更精確（未來可能加 nodejs-project-template 等）。

## D3. 指令對齊方向：補 Taskfile，不刪 CLAUDE.md 的承諾
- **決策**：在 Taskfile.yml 補上 `build`、`test-unit`、`test-integration`、`ef-migration-add`、`ef-database-update` 五個 task，使 CLAUDE.md 的「常用指令」全部可執行。
- **為什麼**：CLAUDE.md「重複指令集中在 Taskfile 管理」的原則本身合理，且這五個 task 都是一行 dotnet 指令的薄包裝，補齊成本低；反向修改（改文件叫大家直接用 dotnet CLI）會放棄集中管理原則。
- **捨棄**：改 CLAUDE.md 為原生 dotnet 指令 → 棄，理由如上。
- **佐證**：本體已實跑 `dotnet build`（0 error）與 `dotnet test`（單元 10/10），包裝內容已驗證可行。

## D4. skill 主檔統一為大寫 SKILL.md
- **決策**：9 個小寫 `skill.md` 以 `git mv` 改為 `SKILL.md`；刪除 `.claude/skills/STRUCTURE-REPORT.md`（它主張小寫、方向相反且內容與現況不符）。
- **為什麼**：Claude Code 規格要求 `SKILL.md`；本機 drvfs 大小寫不敏感遮蔽了問題，但此 repo 是散佈用範本，clone 到 Linux/CI 就會靜默失效——「在我機器上會動」的典型陷阱。
- **捨棄**：維持小寫並依賴 fallback → 棄：fallback 行為未證實（掃描 agent 明列此為不確定項），不能拿散佈範本賭。

## D5. openapi 與實作對齊：移除 tags 殘留端點、補 v1 members 規格
- **決策**：
  1. 從 `doc/openapi.yml` 移除 `/api/v2/tags:cursor`（GetTagsCursor）並重新 codegen——它是複製殘留（回傳型別竟是 Member 分頁），且造成執行期 500 的 blocker。
  2. v1 members 三個已啟用端點補進 openapi.yml（或由使用者確認 v1 屬過渡期不入規格）。
- **為什麼**：範本專案的 API First 原則（規格=契約）是賣點，範本自己規格與實作雙向漂移最傷示範性。tags 端點無 handler、無測試、schema 錯誤，判定為殘留而非未完成功能。
- **捨棄**：為 tags 補實作 → 棄：無任何業務語意（回傳 member 型別），補了只是把死碼做活。
- **不確定性**：v1 members 是否刻意不入規格，需使用者確認（見 risks）。

## D6. 程式碼範本單一來源
- **決策**：`.claude/templates/*.cs`（command-processor 用）與 `.claude/skills/*/assets/*.cs`（skill 用）兩份分歧副本，需人工 diff 擇優合併，保留 `.claude/skills/*/assets/` 為單一來源，`.claude/templates/` 移除並讓 command-processor.md 改指向 assets。
- **為什麼**：skills 是遷移後的主要對外介面（本 session 實際載入的就是 skills）；兩處內容已經 diff 出差異，繼續雙軌必然再漂移。
- **捨棄**：保留 templates/ 為來源 → 兩者擇一皆可行，選 assets 是因 skill 檔案內相對路徑引用 assets 已成慣例；此決策成本對稱，執行時若發現 command-processor 引用太深可反轉。

## D7. Copilot 支援收斂到 .github/copilot-instructions.md 一份
- **決策**：保留 `.github/copilot-instructions.md`（Copilot 唯一自動載入位置）為權威；根層 `copilot-instructions.md` 的獨有規則（「列出每次用了哪些 mcp/agent/skill」）若仍要，回填進 .github 版後刪根層版；`.github/copilot-instructions0-old.md` 直接刪。
- **為什麼**：放錯位置的規則等於不存在——根層版不會被 Copilot 讀取，其獨有規則現在實際上是失效的。
- **捨棄**：全面放棄 Copilot 支援（repo 已遷向 .claude/）→ 需使用者決定，未擅自採用；本決策為「若保留 Copilot」的最小收斂。

## D8. 未 commit 的 60 檔遷移：先收斂為獨立 commit，再開修復分支
- **決策**：建議把目前 staged 的 .github→.claude 遷移單獨 commit（純搬移，不夾修改），之後所有健檢修復在新分支 `health-check-fixes` 上進行。
- **為什麼**：60 檔搬移長期停在 staging 是高風險狀態（任何 reset/checkout 失誤即遺失）；搬移與修復混在同一 commit 會讓 review 無法辨識行為變更。
- **捨棄**：直接在 main 工作區繼續疊修改 → 棄：違反使用者「不動 main」邊界，也不可審查。
- **注意**：commit 動作屬使用者授權範圍，本次健檢未執行任何 git 寫入操作。

## D9. 機敏資料：範本語境下維持現狀、加註警語
- **決策**：`env/local.env` 明文 SA 密碼、TestContainerFactory 硬編測試密碼列為 minor 不強制修；`.mcp.json` 的硬編絕對路徑 `D:\lab\api.template` 需修（可攜性）。
- **為什麼**：本機 dev/短暫測試容器的密碼是範本常見取捨，且 CLAUDE.md 已有「不放機密進 appsettings」原則（appsettings 掃描無機密）；但硬編絕對路徑會讓範本在任何其他機器直接壞掉，屬功能性缺陷而非安全取捨。
