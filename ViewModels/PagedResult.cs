namespace UsersApp.ViewModels
{
    public class PagedResult<T>
    {
        public Guid id { get; set; } = Guid.Empty;
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => (PageNumber * PageSize) < TotalCount;
    }
}
