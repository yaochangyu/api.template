namespace JobBank1111.Job.WebAPI.Member;

public class MemberCommand(MemberRepository repository, 
                           ILogger<MemberCommand> logger)
{
    public async Task InsertAsync(InsertMemberRequest request,CancellationToken cancel = default)
    {
        logger.LogInformation("start");
        await repository.InsertAsync(request,cancel);
        //發送 Event 給 MQ
    }
}