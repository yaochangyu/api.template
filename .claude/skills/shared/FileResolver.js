/**
 * FileResolver - 統一檔案路徑解析工具
 *
 * 支援混合模式：本地優先 + GitHub Raw URL 備援
 *
 * 使用場景：
 * - 開發者已下載整個專案：優先使用本地檔案（快速）
 * - 獨立用戶只安裝 SKILL：自動從 GitHub 下載（靈活）
 * - 離線環境：使用本地快取（可靠）
 *
 * 安全性：
 * - CWE-22 防護：路徑遍歷驗證（禁止 .. 和絕對路徑）
 * - SSRF 防護：GitHub URL 正規化與編碼
 * - 供應鏈安全：下載內容大小限制（2MB）與重定向控制
 */

const fs = require('fs');
const path = require('path');
const os = require('os');
const crypto = require('crypto');

const MAX_DOWNLOAD_BYTES = 2 * 1024 * 1024; // 2 MB 檔案大小限制

class FileResolver {
  constructor(options = {}) {
    this.projectRoot = options.projectRoot || process.cwd();
    this.githubRepo = options.githubRepo || 'yaochangyu/api.template';
    this.githubBranch = options.githubBranch || 'main';
    this.cacheDir = options.cacheDir || path.join(os.homedir(), '.cache', 'skill-resolver');
    this.cacheTTL = options.cacheTTL || 24 * 60 * 60 * 1000; // 24 小時
    this.retryAttempts = options.retryAttempts || 3;
    this.retryDelay = options.retryDelay || 1000; // 1 秒
    this.timeout = options.timeout || 10000; // 10 秒
    this.offlineMode = options.offlineMode || false;
  }

  /**
   * 驗證相對路徑安全性，防止路徑遍歷攻擊（CWE-22）
   *
   * @private
   * @param {string} relativePath - 相對路徑
   * @throws {Error} 如果路徑包含 .. 或絕對路徑
   */
  assertSafeRelativePath(relativePath) {
    // 禁止絕對路徑
    if (path.isAbsolute(relativePath)) {
      throw new Error(`拒絕絕對路徑: "${relativePath}"`);
    }

    // 禁止路徑遍歷（.. 段）
    const normalized = path.normalize(relativePath);
    if (normalized.includes('..') || normalized.startsWith('/')) {
      throw new Error(`拒絕路徑遍歷: "${relativePath}"`);
    }

    // 檢查解析後的路徑是否超出 projectRoot
    const resolvedPath = path.resolve(this.projectRoot, normalized);
    const resolvedRoot = path.resolve(this.projectRoot);

    if (!resolvedPath.startsWith(resolvedRoot + path.sep) && resolvedPath !== resolvedRoot) {
      throw new Error(`路徑超出專案範圍: "${relativePath}"`);
    }
  }

  /**
   * 取得檔案內容
   *
   * 優先級：
   * 1. 本地檔案
   * 2. 本地快取
   * 3. GitHub Raw URL
   *
   * @param {string} relativePath - 相對路徑（例如：dotnet-project-template/doc/openapi.yml）
   * @returns {Promise<string>} 檔案內容
   */
  async getFileContent(relativePath) {
    try {
      // 1. 嘗試本地檔案
      const localPath = this.getFilePath(relativePath);
      if (localPath) {
        console.debug(`[FileResolver] 使用本地檔案: ${localPath}`);
        return fs.readFileSync(localPath, 'utf-8');
      }

      // 2. 嘗試本地快取
      const cachedContent = await this.getFromCache(relativePath);
      if (cachedContent) {
        console.debug(`[FileResolver] 使用快取檔案: ${relativePath}`);
        return cachedContent;
      }

      // 3. 從 GitHub 下載
      if (this.offlineMode) {
        throw new Error(`離線模式：無法取得檔案 "${relativePath}"（本地無檔案或快取）`);
      }

      console.debug(`[FileResolver] 從 GitHub 下載: ${relativePath}`);
      const content = await this.downloadFromGithub(relativePath);

      // 儲存到快取
      await this.saveToCache(relativePath, content);

      return content;
    } catch (error) {
      throw new Error(`無法取得檔案 "${relativePath}": ${error.message}`);
    }
  }

  /**
   * 取得檔案本地路徑（如果存在）
   *
   * 檢查順序：
   * 1. 直接路徑：{projectRoot}/{relativePath}
   * 2. 範本路徑：{projectRoot}/dotnet-project-template/{relativePath}
   * 3. 根目錄（使用檔案名稱）：{projectRoot}/{basename}
   *
   * @param {string} relativePath - 相對路徑
   * @returns {string|null} 如果存在則返回完整路徑，否則返回 null
   * @throws {Error} 如果路徑不安全（路徑遍歷）
   */
  getFilePath(relativePath) {
    // 安全驗證：防止路徑遍歷（CWE-22）
    this.assertSafeRelativePath(relativePath);

    const candidates = [
      // 直接路徑
      path.join(this.projectRoot, relativePath),
      // 範本路徑
      path.join(this.projectRoot, 'dotnet-project-template', relativePath),
      // 根目錄（使用檔案名稱）
      path.join(this.projectRoot, path.basename(relativePath)),
    ];

    for (const candidate of candidates) {
      try {
        if (fs.existsSync(candidate) && fs.statSync(candidate).isFile()) {
          return candidate;
        }
      } catch (error) {
        // 檔案系統錯誤（權限不足等），繼續嘗試下一個候選
        console.debug(`[FileResolver] 無法檢查 ${candidate}: ${error.message}`);
      }
    }

    return null;
  }

  /**
   * 構造 GitHub Raw URL
   *
   * 安全性：
   * - 驗證路徑安全性（防止 SSRF）
   * - URL 編碼檔案路徑
   * - 使用 HTTPS 強制協議
   *
   * @param {string} relativePath - 相對路徑
   * @returns {string} GitHub Raw URL
   * @throws {Error} 如果路徑不安全
   */
  getGithubUrl(relativePath) {
    // 安全驗證：防止 SSRF（OWASP A09:2021）
    this.assertSafeRelativePath(relativePath);

    // 分割路徑並逐段 URL 編碼
    const pathSegments = relativePath.split('/').map(segment => encodeURIComponent(segment));
    const encodedPath = pathSegments.join('/');

    // 驗證 GitHub 倉庫格式（owner/repo）
    if (!this.githubRepo.match(/^[a-zA-Z0-9-]+\/[a-zA-Z0-9._-]+$/)) {
      throw new Error(`無效的 GitHub 倉庫格式: "${this.githubRepo}"`);
    }

    const baseUrl = `https://raw.githubusercontent.com/${this.githubRepo}/${this.githubBranch}`;
    return `${baseUrl}/${encodedPath}`;
  }

  /**
   * 從 GitHub 下載檔案內容（含重試邏輯與安全防護）
   *
   * 安全性防護（供應鏈安全）：
   * - 重定向防護：禁止 HTTP 重定向（防止中間人攻擊）
   * - 大小限制：最大 2MB（防止 DoS）
   * - HTTPS 強制：只接受 HTTPS 連線
   * - 超時控制：防止慢速攻擊
   *
   * @private
   * @param {string} relativePath - 相對路徑
   * @returns {Promise<string>} 檔案內容
   */
  async downloadFromGithub(relativePath) {
    const url = this.getGithubUrl(relativePath);
    let lastError;

    for (let attempt = 1; attempt <= this.retryAttempts; attempt++) {
      try {
        console.debug(`[FileResolver] 下載嘗試 ${attempt}/${this.retryAttempts}: ${url}`);

        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), this.timeout);

        // 重定向防護：禁止重定向（防止中間人攻擊）
        const response = await fetch(url, {
          signal: controller.signal,
          redirect: 'error', // 拒絕重定向
        });
        clearTimeout(timeoutId);

        if (!response.ok) {
          throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        // 檢查 Content-Length（大小限制防護）
        const contentLength = response.headers.get('content-length');
        if (contentLength && parseInt(contentLength) > MAX_DOWNLOAD_BYTES) {
          throw new Error(`檔案過大: ${contentLength} bytes > ${MAX_DOWNLOAD_BYTES} bytes`);
        }

        const content = await response.text();

        // 二次檢查：下載內容大小
        if (content.length > MAX_DOWNLOAD_BYTES) {
          throw new Error(`下載內容過大: ${content.length} bytes > ${MAX_DOWNLOAD_BYTES} bytes`);
        }

        console.debug(`[FileResolver] GitHub 下載成功: ${relativePath} (${content.length} bytes)`);
        return content;
      } catch (error) {
        lastError = error;
        console.debug(`[FileResolver] 下載失敗 (${attempt}/${this.retryAttempts}): ${error.message}`);

        // 區分暫時性錯誤與永久性錯誤
        const isPermanentError = error.message.includes('HTTP 404') ||
                                error.message.includes('拒絕') ||
                                error.message.includes('無效');
        if (isPermanentError) {
          throw error; // 永久性錯誤直接拋出，不再重試
        }

        if (attempt < this.retryAttempts) {
          const delay = this.retryDelay * Math.pow(2, attempt - 1); // 指數退避
          console.debug(`[FileResolver] 等待 ${delay}ms 後重試...`);
          await this.sleep(delay);
        }
      }
    }

    throw new Error(`無法從 GitHub 下載: ${lastError?.message || '未知錯誤'}`);
  }

  /**
   * 從快取取得檔案內容
   *
   * @private
   * @param {string} relativePath - 相對路徑
   * @returns {Promise<string|null>} 檔案內容，如果快取過期或不存在則返回 null
   */
  async getFromCache(relativePath) {
    try {
      const cacheKey = this.getCacheKey(relativePath);
      const cacheFile = path.join(this.cacheDir, cacheKey);

      if (!fs.existsSync(cacheFile)) {
        return null;
      }

      const stats = fs.statSync(cacheFile);
      const age = Date.now() - stats.mtimeMs;

      if (age > this.cacheTTL) {
        console.debug(`[FileResolver] 快取已過期: ${relativePath}`);
        return null;
      }

      const content = fs.readFileSync(cacheFile, 'utf-8');
      return content;
    } catch (error) {
      console.debug(`[FileResolver] 快取讀取失敗: ${error.message}`);
      return null;
    }
  }

  /**
   * 保存內容到快取
   *
   * @private
   * @param {string} relativePath - 相對路徑
   * @param {string} content - 檔案內容
   * @returns {Promise<void>}
   */
  async saveToCache(relativePath, content) {
    try {
      // 建立快取目錄
      if (!fs.existsSync(this.cacheDir)) {
        fs.mkdirSync(this.cacheDir, { recursive: true });
      }

      const cacheKey = this.getCacheKey(relativePath);
      const cacheFile = path.join(this.cacheDir, cacheKey);

      fs.writeFileSync(cacheFile, content, 'utf-8');
      console.debug(`[FileResolver] 已保存到快取: ${relativePath}`);
    } catch (error) {
      console.debug(`[FileResolver] 快取保存失敗: ${error.message}`);
      // 快取失敗不應該中斷主流程
    }
  }

  /**
   * 計算快取鍵
   *
   * @private
   * @param {string} relativePath - 相對路徑
   * @returns {string} 快取鍵（SHA256 雜湊）
   */
  getCacheKey(relativePath) {
    const key = `${this.githubRepo}:${relativePath}`;
    return crypto.createHash('sha256').update(key).digest('hex');
  }

  /**
   * 清空快取
   *
   * @returns {Promise<void>}
   */
  async clearCache() {
    try {
      if (fs.existsSync(this.cacheDir)) {
        fs.rmSync(this.cacheDir, { recursive: true, force: true });
        console.log(`[FileResolver] 已清空快取目錄: ${this.cacheDir}`);
      }
    } catch (error) {
      console.error(`[FileResolver] 清空快取失敗: ${error.message}`);
      throw error;
    }
  }

  /**
   * 列出快取中的所有檔案
   *
   * @returns {Promise<Array>} 快取檔案列表
   */
  async listCache() {
    try {
      if (!fs.existsSync(this.cacheDir)) {
        return [];
      }

      const files = fs.readdirSync(this.cacheDir);
      return files.map((file) => {
        const filePath = path.join(this.cacheDir, file);
        const stats = fs.statSync(filePath);
        return {
          name: file,
          size: stats.size,
          modified: new Date(stats.mtimeMs),
          age: Date.now() - stats.mtimeMs,
        };
      });
    } catch (error) {
      console.error(`[FileResolver] 列出快取失敗: ${error.message}`);
      return [];
    }
  }

  /**
   * 辅助函数：延遲
   *
   * @private
   * @param {number} ms - 毫秒數
   * @returns {Promise<void>}
   */
  sleep(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}

module.exports = FileResolver;

/**
 * CLI 入口 - 支援直接執行 FileResolver.js
 *
 * 用法：
 *   node FileResolver.js get-content <path>          # 取得檔案內容
 *   node FileResolver.js get-path <path>              # 取得本地路徑
 *   node FileResolver.js get-github-url <path>        # 取得 GitHub URL
 *   node FileResolver.js clear-cache                  # 清空快取
 *   node FileResolver.js list-cache                   # 列出快取
 *
 * 範例：
 *   node FileResolver.js get-content dotnet-project-template/doc/openapi.yml
 *   node FileResolver.js get-github-url dotnet-project-template/doc/openapi.yml
 */
if (require.main === module) {
  (async () => {
    const args = process.argv.slice(2);
    const command = args[0];
    const relativePath = args[1];

    if (!command) {
      console.error('使用方式:');
      console.error('  node FileResolver.js get-content <path>');
      console.error('  node FileResolver.js get-path <path>');
      console.error('  node FileResolver.js get-github-url <path>');
      console.error('  node FileResolver.js clear-cache');
      console.error('  node FileResolver.js list-cache');
      process.exit(1);
    }

    try {
      const resolver = new FileResolver();

      switch (command) {
        case 'get-content':
          if (!relativePath) throw new Error('缺少路徑參數');
          const content = await resolver.getFileContent(relativePath);
          console.log(content);
          break;

        case 'get-path':
          if (!relativePath) throw new Error('缺少路徑參數');
          const filePath = resolver.getFilePath(relativePath);
          if (filePath) {
            console.log(filePath);
          } else {
            console.log('(本地未找到，需從 GitHub 下載)');
            process.exit(1);
          }
          break;

        case 'get-github-url':
          if (!relativePath) throw new Error('缺少路徑參數');
          const url = resolver.getGithubUrl(relativePath);
          console.log(url);
          break;

        case 'clear-cache':
          await resolver.clearCache();
          console.log('快取已清空');
          break;

        case 'list-cache':
          const files = await resolver.listCache();
          if (files.length === 0) {
            console.log('快取為空');
          } else {
            console.log('快取檔案:');
            files.forEach(file => {
              const ageMs = file.age;
              const ageMins = Math.floor(ageMs / 60000);
              console.log(`  ${file.name} (${file.size} bytes, ${ageMins}分鐘前修改)`);
            });
          }
          break;

        default:
          console.error(`未知命令: ${command}`);
          process.exit(1);
      }
    } catch (error) {
      console.error(`錯誤: ${error.message}`);
      process.exit(1);
    }
  })();
}
