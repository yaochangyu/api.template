using CSharpFunctionalExtensions;
using JobBank1111.Infrastructure;
using JobBank1111.Infrastructure.TraceContext;
using JobBank1111.Job.DB;
using Microsoft.EntityFrameworkCore;

namespace {{namespace}};

public class {{feature-name}}Repository(
    IDbContextFactory<MemberDbContext> dbContextFactory,
    IContextGetter<TraceContext?> contextGetter,
    TimeProvider timeProvider,
    IUuidProvider uuidProvider)
{
    public async Task<Result<{{feature-name}}Entity, Failure>> CreateAsync(
        Create{{feature-name}}Request request,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
            var now = timeProvider.GetUtcNow();
            var traceContext = contextGetter.Get();
            
            var entity = new {{feature-name}}Entity
            {
                Id = uuidProvider.NewId(),
                Name = request.Name,
                // Map other properties from request
                CreatedAt = now,
                CreatedBy = traceContext?.UserId,
                ChangedAt = now,
                ChangedBy = traceContext?.UserId
            };

            dbContext.Add(entity);
            await dbContext.SaveChangesAsync(cancel);
            
            return Result.Success<{{feature-name}}Entity, Failure>(entity);
        }
        catch (Exception ex)
        {
            var traceContext = contextGetter.Get();
            return Result.Failure<{{feature-name}}Entity, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "An unexpected error occurred while accessing the database.",
                Data = request,
                Exception = ex,
                TraceId = traceContext?.TraceId
            });
        }
    }

    public async Task<Result<{{feature-name}}Entity, Failure>> GetByIdAsync(Guid id, CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
            var entity = await dbContext.Set<{{feature-name}}Entity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancel);

            if (entity is null)
            {
                return Result.Failure<{{feature-name}}Entity, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = "{{feature-name}} not found."
                });
            }

            return Result.Success<{{feature-name}}Entity, Failure>(entity);
        }
        catch (Exception ex)
        {
            var traceContext = contextGetter.Get();
            return Result.Failure<{{feature-name}}Entity, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "An unexpected error occurred while accessing the database.",
                Data = new { id },
                Exception = ex,
                TraceId = traceContext?.TraceId
            });
        }
    }
}
