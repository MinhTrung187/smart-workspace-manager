using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartWorkspaceManager.Application.Common
{
    public sealed class PagedList<T>
    {
        public int CurrentPage { get; init; }
        public int TotalPages { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public IReadOnlyList<T> Items { get; init; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = count;
            Items = items.AsReadOnly();
        }

        public static PagedList<T> Create(IEnumerable<T> source, int count, int pageNumber, int pageSize)
        {
            return new PagedList<T>(source.ToList(), count, pageNumber, pageSize);
        }
    }

    public sealed class Metadata
    {
        public int CurrentPage { get; init; }
        public int TotalPages { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
    