using CSharpFunctionalExtensions;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI.Member;

public class MemberHandler(
    MemberRepository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<MemberHandler> logger)
{
    public async Task<Result<Member, Failure>>
        InsertAsync(InsertMemberRequest request,
                    CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        var queryResult = await repository.QueryEmailAsync(request.Email, cancel);
        if (queryResult.IsFailure)
        {
            return queryResult;
        }

        var srcMember = queryResult.Value;

        //前置條件檢查，可以用 Fluent Pattern 重構
        var validateResult = Result.Success<Member, Failure>(srcMember);
        validateResult = ValidateEmail(validateResult, request);
        validateResult = ValidateName(validateResult, request);
        if (validateResult.IsFailure)
        {
            return validateResult;
        }

        var insertResult = await repository.InsertAsync(request, cancel);
        if (insertResult.IsFailure)
        {
            return Result.Failure<Member, Failure>(insertResult.Error);
        }

        var success = Result.Success<Member, Failure>(srcMember);
        return success;
    }

    // 檢查是否有重複的 Email
    private Result<Member, Failure>
        ValidateEmail(Result<Member, Failure> previousResult,
                      InsertMemberRequest dest)
    {
        if (previousResult.IsFailure)
        {
            return previousResult;
        }

        var src = previousResult.Value;
        if (src == null)
        {
            return Result.Success<Member, Failure>(src);
        }

        var traceContext = traceContextGetter.Get();
        if (src.Email == dest.Email)
        {
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.DuplicateEmail),
                Message = "Email 重複",
                Data = src,
                TraceId = traceContext?.TraceId
            });
        }

        return Result.Success<Member, Failure>(src);
    }

    // 檢查是否有重複的 Name
    private Result<Member, Failure>
        ValidateName(Result<Member, Failure> previousResult,
                     InsertMemberRequest dest)
    {
        if (previousResult.IsFailure)
        {
            return previousResult;
        }

        var src = previousResult.Value;
        if (src == null)
        {
            return Result.Success<Member, Failure>(src);
        }

        var traceContext = traceContextGetter.Get();
        if (src.Name == dest.Name)
        {
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Name 重複",
                Data = src,
                TraceId = traceContext?.TraceId
            });
        }

        return Result.Success<Member, Failure>(src);
    }

    public async Task<Result<PaginatedList<GetMemberResponse>, Failure>>
        GetMemberOffsetAsync(int pageIndex, int pageSize, bool noCache = true, CancellationToken cancel = default)
    {
        var result = await repository.GetMemberOffsetAsync(pageIndex, pageSize, noCache, cancel);
        return result;
    }

    public async Task<Result<CursorPaginatedList<GetMemberResponse>, Failure>>
        GetMembersCursorAsync(int pageSize, string nextPageToken, bool noCache = true, CancellationToken cancel = default)
    {
        var result = await repository.GetMembersCursorAsync(pageSize, nextPageToken, noCache, cancel);
        return result;
    }
}