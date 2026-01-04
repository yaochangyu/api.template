namespace JobBank1111.Job.WebAPI;

public enum FailureCode
{
    Unauthorized,
    DbError,
    DuplicateEmail,
    DbConcurrency,
    ValidationError,
    InvalidOperation,
    Timeout,
    InternalServerError,
    Unknown
}