using System.Net;
using JobBank1111.Job.WebAPI;
using Xunit;

namespace JobBank1111.Job.Test;

public class FailureCodeMapperTests
{
    [Theory]
    [InlineData(nameof(FailureCode.Unauthorized), HttpStatusCode.Unauthorized)]
    [InlineData(nameof(FailureCode.DbError), HttpStatusCode.InternalServerError)]
    [InlineData(nameof(FailureCode.DuplicateEmail), HttpStatusCode.Conflict)]
    [InlineData("UnknownCode", HttpStatusCode.InternalServerError)]
    public void GetHttpStatusCode_ShouldReturnCorrectStatusCode(string failureCode, HttpStatusCode expected)
    {
        var result = FailureCodeMapper.GetHttpStatusCode(failureCode);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetHttpStatusCode_WithFailureObject_ShouldReturnCorrectStatusCode()
    {
        var failure = new Failure 
        { 
            Code = nameof(FailureCode.DuplicateEmail),
            Message = "Test message"
        };

        var result = FailureCodeMapper.GetHttpStatusCode(failure);
        
        Assert.Equal(HttpStatusCode.Conflict, result);
    }
}