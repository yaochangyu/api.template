using CSharpFunctionalExtensions;
using JobBank1111.Infrastructure.TraceContext;

namespace {{namespace}};

public class {{feature-name}}Handler(
    {{feature-name}}Repository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<{{feature-name}}Handler> logger)
{
    public async Task<Result<{{feature-name}}Response, Failure>>
        CreateAsync(Create{{feature-name}}Request request,
                    CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        // Example of validation: check for duplicates
        var existingItemResult = await repository.GetByNameAsync(request.Name, cancel);
        if (existingItemResult.IsSuccess && existingItemResult.Value != null)
        {
            return Result.Failure<{{feature-name}}Response, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "{{feature-name}} with the same name already exists.",
                TraceId = traceContext?.TraceId
            });
        }

        var insertResult = await repository.CreateAsync(request, cancel);
        
        return insertResult.Map(entity => new {{feature-name}}Response 
        {
            Id = entity.Id,
            Name = entity.Name
            // Map other properties
        });
    }

    public async Task<Result<{{feature-name}}Response, Failure>>
        GetByIdAsync(Guid id, CancellationToken cancel = default)
    {
        var result = await repository.GetByIdAsync(id, cancel);

        return result.Map(entity => new {{feature-name}}Response 
        {
            Id = entity.Id,
            Name = entity.Name
            // Map other properties
        });
    }
}
