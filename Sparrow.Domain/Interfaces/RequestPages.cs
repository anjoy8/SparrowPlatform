namespace SparrowPlatform.Domain.Interfaces
{
    public class RequestPages
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
        public string Sorting { get; set; } = "Id";
        public bool IsDesc { get; set; } = false;
        public string Language { get; set; } = "en";
    }
}
