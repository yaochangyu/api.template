namespace JobBank1111.Infrastructure;

public interface IUuidProvider
{
    public string NewId();
}

public class UuidProvider : IUuidProvider
{
    public string NewId() => Guid.NewGuid().ToString();
}