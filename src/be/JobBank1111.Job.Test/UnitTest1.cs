using JobBank1111.Job.WebAPI;
using JobBank1111.Job.WebAPI.Contract;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace JobBank1111.Job.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var data = new WebAPI.PaginatedList<WebAPI.Member.GetMemberResponse>();
        
        var json = "{\"items\":[{\"id\":\"1a6bd961-7a8b-470d-9282-2420c1daa211\",\"name\":\"yao\",\"age\":20,\"email\":\"yao@9527\"},{\"id\":\"93737ff9-a162-4d2e-92ac-b1944f5cce90\",\"name\":\"yao2\",\"age\":18,\"email\":\"9527@yao\"}],\"pageIndex\":0,\"totalPages\":1,\"hasPreviousPage\":false,\"hasNextPage\":true}";
        var paginatedList = JsonSerializer.Deserialize<WebAPI.PaginatedList<WebAPI.Member.GetMemberResponse>>(json);
        var deserializeObject = JsonConvert.DeserializeObject<WebAPI.PaginatedList<WebAPI.Member.GetMemberResponse>>(json);
    }
}