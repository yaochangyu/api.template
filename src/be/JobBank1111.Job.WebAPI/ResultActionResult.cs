using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI;

public class ResultActionResult<TSuccess, TFailure> : ActionResult 
    where TFailure : class
{
    private readonly Result<TSuccess, TFailure> _result;

    public ResultActionResult(Result<TSuccess, TFailure> result)
    {
        _result = result;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var objectResult = _result.IsSuccess 
            ? CreateSuccessResult(_result.Value)
            : CreateFailureResult(_result.Error);

        await objectResult.ExecuteResultAsync(context);
    }

    private ObjectResult CreateSuccessResult(TSuccess value)
    {
        return new ObjectResult(value)
        {
            StatusCode = StatusCodes.Status200OK
        };
    }

    private ObjectResult CreateFailureResult(TFailure error)
    {
        var statusCode = error is Failure failure 
            ? FailureCodeMapper.GetHttpStatusCode(failure)
            : System.Net.HttpStatusCode.BadRequest;

        return new ObjectResult(error)
        {
            StatusCode = (int)statusCode
        };
    }
}

public static class ResultExtensions
{
    public static ResultActionResult<TSuccess, TFailure> ToActionResult<TSuccess, TFailure>(
        this Result<TSuccess, TFailure> result) 
        where TFailure : class
    {
        return new ResultActionResult<TSuccess, TFailure>(result);
    }
}