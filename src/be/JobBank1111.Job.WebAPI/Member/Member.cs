namespace JobBank1111.Job.WebAPI.Member;

public class Member
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public int? Age { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? ChangedAt { get; set; }

    public string? ChangedBy { get; set; }

    public string Email { get; set; }
}