# 容器化與部署指引

本文件包含 Docker 容器化、CI/CD 與生產環境設定管理的最佳實務。

## Docker 容器化

#### 多階段建置
使用多階段 Dockerfile 減少映像大小，分離建置環境與執行環境。

#### 安全性考量
- 使用 non-root 使用者執行
- 最小化映像大小
- 定期更新基礎映像

## CI/CD 管線

支援 GitHub Actions、Azure DevOps 等 CI/CD 工具，自動化測試、建置與部署流程。

## 生產環境設定管理

#### 環境變數與機密管理
- 開發環境：.NET User Secrets、`env/local.env`
- 容器環境：Docker/K8s Secrets
- 雲端環境：Azure Key Vault 等祕密管家

#### Kubernetes 部署
支援 Kubernetes 部署，包含 Deployment、Service、HPA 等資源配置。
