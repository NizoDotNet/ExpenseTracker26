using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Shared;

public class PagedResult<T>
{
    public PagedResult(int page, int pageSize, int total, List<T> values)
    {
        Page = page;
        PageSize = pageSize;
        Total = total;
        Values = values;
    }

    public int Page { get; private set; }
    public int PageSize { get; private set; }
    public int Total { get; private set; }
    public List<T> Values { get; private set; }

    public static async Task<PagedResult<T>> Create(IQueryable<T> query, int page, int pageSize)
    {
        int count = await query.CountAsync();
        var elements = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new(page, pageSize, count, elements);
    }


}
