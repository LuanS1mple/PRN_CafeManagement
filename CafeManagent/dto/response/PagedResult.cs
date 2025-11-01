namespace CafeManagent.dto.response
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int Page { get; init; }
        public int Size { get; init; }
        public int TotalItems { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, Size));
    }
}
