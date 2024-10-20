using JobBank1111.Infrastructure;
using JobBank1111.Infrastructure.TraceContext;
using JobBank1111.Job.DB;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.WebAPI.Member;

public class MemberRepository(
    ILogger<MemberController> logger,
    IContextGetter<TraceContext?> authContextGetter,
    IDbContextFactory<MemberDbContext> dbContextFactory,
    TimeProvider timeProvider,
    IUuidProvider uuidProvider)
{
    public async Task<int> InsertAsync(InsertMemberRequest request,
                                       CancellationToken cancel = default)
    {
        // throw new DbUpdateConcurrencyException("資料衝突了");

        var now = timeProvider.GetUtcNow();
        var userId = authContextGetter.Get().UserId;
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        var toDb = new DB.Member
        {
            Id = uuidProvider.NewId(),
            Name = request.Name,
            Age = request.Age,
            Email = request.Email,
            CreatedAt = now,
            CreatedBy = userId,
            ChangedAt = now,
            ChangedBy = userId
        };
        var entityEntry = dbContext.Members.Add(toDb);
        return await dbContext.SaveChangesAsync(cancel);
    }

    public async Task<Member> QueryEmailAsync(string email,
                                              CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        var query = dbContext.Members
            .Where(p => p.Email == email)
            .Select(p => new Member
            {
                Id = p.Id,
                Name = p.Name,
                Age = p.Age,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                ChangedAt = p.ChangedAt,
                ChangedBy = p.ChangedBy
            });

        var result = await query.TagWith($"{nameof(MemberRepository)}.{nameof(this.QueryEmailAsync)}({email})")
            .AsNoTracking()
            .FirstOrDefaultAsync(cancel);
        return result;
    }
}