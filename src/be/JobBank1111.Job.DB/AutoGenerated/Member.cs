using System;
using System.Collections.Generic;

namespace JobBank1111.Job.DB;

public partial class Member
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public int? Age { get; set; }

    public long SequenceId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? ChangedAt { get; set; }

    public string? ChangedBy { get; set; }
}
