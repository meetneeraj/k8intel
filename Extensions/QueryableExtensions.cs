using K8Intel.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace K8Intel.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query, int pageNumber, int pageSize)
        {
            // 1. Get the total count of items.
            var totalCount = await query.CountAsync();
            
            // 2. Create the result object and populate metadata.
            var result = new PagedResult<T>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            
            // 3. Get the paginated items from the database.
            var pagedItems = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            result.Items = pagedItems;
            
            return result;
        }
    }
}