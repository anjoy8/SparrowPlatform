using System.Collections.Generic;

namespace SparrowPlatform.Application.ViewModels
{
    /// <summary>
    /// page model with t.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageModel<T> : ResponseModelBase<T>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
    }
    public class ResponseModelBase<T>
    {
        public List<T> data { get; set; }
    }
}
