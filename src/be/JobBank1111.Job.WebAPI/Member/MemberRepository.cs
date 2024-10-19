using JobBank1111.Infrastructure.TraceContext;
using JobBank1111.Job.DB;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.WebAPI.Member;

public class MemberRepository(
    ILogger<MemberController> logger,
    IContextGetter<TraceContext?> authContextGetter,
    IDbContextFactory<MemberDbContext> dbContextFactory,
    TimeProvider timeProvider)
{
    public async Task<int> InsertAsync(InsertMemberRequest request, 
                                       CancellationToken cancel = default)
    {
        var now = timeProvider.GetUtcNow();
        var userId = authContextGetter.Get().UserId;
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        dbContext.Members.Add(new DB.Member
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Age = request.Age,
            CreatedAt = now,
            CreatedBy = userId,
            ChangedAt = now,
            ChangedBy = userId
        });
        return await dbContext.SaveChangesAsync(cancel);
    }
}