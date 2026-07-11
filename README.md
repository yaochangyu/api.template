# api.template — .NET WebAPI Development Template

**Language**: English | [繁體中文](./README.zh-TW.md)

A comprehensive .NET WebAPI development template implementing **Clean Architecture**, **layered architecture**, and **Result Pattern** best practices.

## 📋 Quick Start

### Prerequisites
- .NET 8+ SDK
- Docker & Docker Compose (for database and cache test environments)
- Git

### Basic Steps

1. **Clone the project**
   ```bash
   git clone https://github.com/yaochangyu/api.template.git
   cd api.template/dotnet-project-template
   ```

2. **Configure environment variables**
   ```bash
   cp env/local.env .env
   # Edit .env to set local database connection strings, Redis address, etc.
   ```

3. **Start development environment**
   ```bash
   docker-compose up -d      # Start SQL Server, Redis, etc.
   dotnet build               # Build the solution
   dotnet run --project src/be/JobBank1111.Job.WebAPI
   ```

4. **Verify API is running**
   ```bash
   curl http://localhost:5000/api/health
   ```

5. **Run tests**
   ```bash
   dotnet test                # Execute all unit and integration tests
   ```

## 📁 Directory Structure

```
api.template/
├── .claude/                          # Development tools & AI assistant guides
│   ├── CLAUDE.md                     # AI assistant behavior rules & plan management
│   ├── development-rules.md          # Development rules & best practices
│   ├── decision-framework.md         # Decision logic for API development
│   ├── skills/                       # Development helper skill definitions (16 total)
│   │   ├── api-development/          # API endpoint design & decision logic
│   │   ├── handler/                  # Business logic layer implementation
│   │   ├── repository-design/        # Data access layer design
│   │   ├── error-handling/           # Unified error handling patterns
│   │   ├── middleware/               # HTTP middleware implementation
│   │   ├── ef-core/                  # EF Core optimization guide
│   │   ├── bdd-testing/              # BDD integration tests (Reqnroll/Gherkin)
│   │   └── [9 other skills]
│   └── agents/                       # AI Agent workflows (architecture review, feature development, etc.)
│
├── dotnet-project-template/          # Main .NET project
│   ├── src/be/                       # Backend code
│   │   ├── JobBank1111.Infrastructure/    # Shared infrastructure (caching, logging, TraceContext)
│   │   ├── JobBank1111.Job.Contract/      # API contracts & auto-generated Client
│   │   ├── JobBank1111.Job.DB/            # EF Core DbContext & Entities
│   │   ├── JobBank1111.Job.WebAPI/        # API Controllers & middleware
│   │   ├── JobBank1111.Job.Test/          # Unit tests (Xunit)
│   │   └── JobBank1111.Job.IntegrationTest/ # Integration tests (Reqnroll/BDD)
│   │
│   ├── doc/                          # API specification documents
│   │   └── openapi.yml               # OpenAPI specification (Swagger)
│   │
│   ├── env/                          # Environment configuration
│   │   ├── .template-config.json     # Template configuration
│   │   └── local.env                 # Local environment variables
│   │
│   ├── k8s/                          # Kubernetes deployment configuration
│   ├── docker-compose.yml            # Local development environment (DB, Redis)
│   ├── Taskfile.yml                  # Common tasks (build, test, deploy)
│   └── best-practices.md             # Project best practices guide
│
├── CLAUDE.md                         # AI assistant behavior rules & plan management
├── tree.md                           # Complete file listing
└── .archive/                         # Completed plan documents (for reference)
```

## 🏗️ Core Architecture

### Layered Design

```
HTTP Request
     ↓
[ Middleware Layer ]      # Logging, authentication, TraceContext, error handling
     ↓
[ Controller Layer ]      # HTTP routing & request validation (MemberController.cs)
     ↓
[ Handler Layer ]         # Business logic (MemberHandler.cs)
     ↓
[ Repository Layer ]      # Data access abstraction (IMemberRepository)
     ↓
[ EF Core DbContext ]     # ORM implementation
     ↓
[ Database ]              # SQL Server / PostgreSQL
```

### Core Concepts

#### 1. **API Development Approach**

Choose the appropriate development approach based on your project stage (**must choose one, do not mix**):

| Approach | When to Use | Example File |
|----------|-----------|----------|
| **API First** | API specification is finalized, auto-generate Controller | `MemberV1ControllerImpl.cs` |
| **Code First** | Implement Controller directly without spec auto-generation | `MemberController.cs` |

👉 Detailed decision logic: see [.claude/decision-framework.md](./.claude/decision-framework.md#api開發流程決策)

#### 2. **Result Pattern (Unified Result Type)**

All API endpoints return a unified `Result<T>` structure:

```csharp
{
  "isSuccess": true,
  "data": { ... },
  "failure": null,
  "traceId": "550e8400-e29b-41d4-a716-446655440000"
}
```

- **Benefits**: Unified error handling, complete trace context, automatic logging
- **Implementation**: see [/error-handling SKILL](./.claude/skills/error-handling/SKILL.md)

#### 3. **TraceContext (Request Tracing)**

Every request automatically gets a unique TraceId that flows through the entire call chain:

```csharp
public class TraceContext
{
    public string TraceId { get; set; }  // Globally unique identifier
    public string UserId { get; set; }   // Current user
    public DateTime RequestTime { get; set; }
}
```

Facilitates log aggregation, performance analysis, and fault diagnosis.

#### 4. **Immutable Objects**

Use C# record types and init properties to ensure objects cannot be modified after creation:

```csharp
public record MemberResponse(
    int Id,
    string Name,
    string Email
);
```

#### 5. **Async I/O**

All database and network operations use async/await:

```csharp
public async Task<Result<MemberResponse>> GetMemberAsync(int id)
{
    var member = await _repository.GetByIdAsync(id);
    return member != null 
        ? Result.Ok(new MemberResponse(member.Id, member.Name, member.Email))
        : Result.Fail<MemberResponse>("Member not found");
}
```

## 🛠️ Development Workflow

### Typical API Endpoint Implementation Flow

Suppose you need to implement a "Get Member List" endpoint:

1. **Choose development approach** (API First or Code First)
   ```bash
   /api-development
   ```

2. **Design data access layer**
   ```bash
   /repository-design
   ```

3. **Implement business logic layer**
   ```bash
   /handler
   ```

4. **Implement Controller**
   ```bash
   /handler   # Also generates Controller reference
   ```

5. **Implement BDD tests**
   ```bash
   /bdd-testing
   ```

6. **Verify completeness**
   ```bash
   dotnet test
   dotnet build
   ```

## 📖 Advanced Guides

Consult corresponding documentation based on your task:

| Task | Related Document | Location |
|------|---------|----------|
| Understanding development workflow | SKILL usage guide | [CLAUDE.md](./CLAUDE.md) |
| Designing API endpoints | API development decision framework | [.claude/decision-framework.md](./.claude/decision-framework.md) |
| Implementing Controller | Development rules & best practices | [.claude/development-rules.md](./.claude/development-rules.md) |
| Writing integration tests | BDD testing guide | [.claude/skills/bdd-testing/SKILL.md](./.claude/skills/bdd-testing/SKILL.md) |
| Caching design | Caching strategy decision | [.claude/decision-framework.md#caching-strategy](./.claude/decision-framework.md) |
| Security checks | Security scanning tools | [.claude/skills/security-*](./.claude/skills/) |

## 📚 Important Documents

- **[CLAUDE.md](./CLAUDE.md)** — AI assistant behavior rules, plan management, advanced topics index
- **[.claude/development-rules.md](./.claude/development-rules.md)** — Development rules and best practices
- **[dotnet-project-template/best-practices.md](./dotnet-project-template/best-practices.md)** — .NET project best practices
- **[dotnet-project-template/docs/development/reqnroll-best-practices.md](./dotnet-project-template/docs/development/reqnroll-best-practices.md)** — BDD/Gherkin testing guide

## 🔧 Common Commands

```bash
# Development
dotnet build                                    # Build the entire solution
dotnet run --project src/be/JobBank1111.Job.WebAPI   # Start API server
dotnet watch run --project src/be/JobBank1111.Job.WebAPI  # Hot reload mode

# Testing
dotnet test                                     # Execute all tests (unit + integration)
dotnet test --filter FullyQualifiedName~Member # Run specific test

# Database migrations (using EF Core)
dotnet ef migrations add CreateMemberTable --project src/be/JobBank1111.Job.DB
dotnet ef database update --project src/be/JobBank1111.Job.DB

# Docker
docker-compose up -d                            # Start local development environment
docker-compose down                             # Stop development environment
docker-compose logs -f                          # View container logs
```

## 🎯 Development Policy

### Must Follow

- ✅ **Layered Architecture**: Controller → Handler → Repository → DbContext
- ✅ **Immutable Objects**: Use records and init properties
- ✅ **Async Operations**: All I/O uses async/await
- ✅ **Result Pattern**: Unified success/failure type
- ✅ **TraceContext**: Every request carries a TraceId
- ✅ **BDD Tests**: Integration tests use Gherkin/Reqnroll

### Prohibited

- ❌ Mixing API First and Code First approaches
- ❌ Skipping async/await (synchronous I/O blocks threads)
- ❌ Writing business logic directly in Controllers (violates layering)
- ❌ Mocking databases in integration tests (use real Docker containers)

## 📝 Lessons Learned: .NET 10 Upgrade (2026-07-11)

### Problem
HTTP 500 errors appeared in 3/8 integration tests after .NET 10 upgrade, initially suspected as JSON serialization issues.

### Root Cause Analysis
❌ **Wrong assumption**: Blamed .NET 10's `System.Text.Json` async serialization and `PipeWriter.UnflushedBytes`
✅ **Actual cause**: DI container unable to resolve `IMemberController` dependency required by auto-generated NSwag wrapper

### Failed Approach
- Added `SynchronousJsonOutputFormatter` custom JSON formatter
- Added global `JsonOptions` configuration
- Attempted workarounds in `Program.cs` AddControllers configuration

Result: Unnecessary code bloat, but didn't fix the underlying problem.

### Correct Solution
1. Registered `IMemberController` → `MemberController` in DI container
2. Implemented `IMemberController` interface in v2 Controller
3. Then reverted back to Code First (v2 shouldn't implement API First contracts)

### Key Lessons

| Lesson | Why It Matters |
|--------|---|
| **Framework upgrades = minimal config changes** | Only modify Swagger (→ OpenAPI), keep everything else as-is |
| **Don't assume the symptom = root cause** | HTTP 500 can hide DI, routing, or serialization issues—check DI first |
| **API First (v1) ≠ Code First (v2)** | v1 implements interfaces; v2 uses attributes directly |
| **NSwag auto-generates wrapper Controllers** | Manual Controller must either implement the interface OR not exist simultaneously |
| **Remove workarounds once root cause is fixed** | Left-over defensive code increases maintenance burden |

### Final State
- ✅ All 8 integration tests pass
- ✅ No unnecessary JSON serialization workarounds
- ✅ Program.cs is clean and minimal
- ✅ v2 Controller uses Code First (clean attributes-based approach)

## 📞 Questions & Support

- Workflow questions: Consult [CLAUDE.md](./CLAUDE.md) and SKILL documentation
- Development decisions: Refer to [.claude/decision-framework.md](./.claude/decision-framework.md)
- Technical issues: Check [dotnet-project-template/best-practices.md](./dotnet-project-template/best-practices.md)

---

**Version**: api.template v1.0  
**Last Updated**: 2026-07-11  
**Purpose**: For both human and AI Agent reading
