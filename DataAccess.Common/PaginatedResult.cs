namespace DataAccess.Common;

public interface IPaginatedResult<in T>
{
}

public class PaginatedResult<T> : IPaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int DataLength { get; set; }

    public static PaginatedResult<T> Init() => new() { Data = new List<T>(), DataLength = 0 };
}
