namespace CafeManagent.dto.response
{
    public class Paging<T>
    {
        public int Total { get; set; }
        public int PageIndex { get; set; } = 1;
        public List<T> Data { get; set; }
    }
}
