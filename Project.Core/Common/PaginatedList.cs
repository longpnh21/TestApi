using System;
using System.Collections.Generic;

namespace Project.Core.Common
{
    public class PaginatedList<T>
    {
        public List<T> Result { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageIndex > 2;
        public bool HasNextPage => PageIndex < TotalPages;
        public PaginatedList(IEnumerable<T> list, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Result = new List<T>();
            Result.AddRange(list);
        }

        public PaginatedList(IEnumerable<T> list, int count)
        {
            PageIndex = 1;
            PageSize = count;
            TotalPages = 1;
            Result.AddRange(list);
        }
    }
}
