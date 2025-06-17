using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Skeleton.EntityFrameworkCore
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// For tracking where queries come from in code.  Adds a comment to the generated SQL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="lineNumber"></param>
        /// <param name="filePath"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static IQueryable<T> TagWithSource<T>(this IQueryable<T> queryable, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            return queryable.TagWith($"{memberName} - {filePath}:{lineNumber}");
        }
    }
}
