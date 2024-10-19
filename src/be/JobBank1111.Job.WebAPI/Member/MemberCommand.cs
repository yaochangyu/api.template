using CSharpFunctionalExtensions;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI.Member;

public class MemberCommand(
    MemberRepository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<MemberCommand> logger)
{
    public async Task<Result<DB.Member, Failure>> InsertAsync(InsertMemberRequest request,
                                                              CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        var member = new DB.Member() { Id = Guid.NewGuid() };

        var failure = new Failure
        {
            Code = nameof(FailureCode.DbError), Message = "資料庫錯誤", Data = request, Exception = new Exception("看不到我"),
        };
        traceContext.SetTraceId(failure);

        var success = Result.Success<DB.Member, Failure>(member);
        var failed = Result.Failure<DB.Member, Failure>(failure);

        await repository.InsertAsync(request, cancel);

        //發送 Event 給 MQ

        return success;
    }
}