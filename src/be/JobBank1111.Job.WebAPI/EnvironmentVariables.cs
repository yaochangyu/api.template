using JobBank1111.Infrastructure;

namespace JobBank1111.Job.WebAPI;

public record ASPNETCORE_ENVIRONMENT : EnvironmentVariableBase;

public record SYS_DATABASE_CONNECTION_STRING : EnvironmentVariableBase;

public record SYS_REDIS_URL : EnvironmentVariableBase;

public record DEFAULT_CACHE_EXPIRATION : EnvironmentVariableBase;

public record EXTERNAL_API : EnvironmentVariableBase;