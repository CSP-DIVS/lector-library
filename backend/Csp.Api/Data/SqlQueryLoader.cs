using System.Collections.Concurrent;

namespace Csp.Api.Data
{
    /// <summary>
    /// Utility class for loading SQL queries from .sql files with caching support
    /// </summary>
    public static class SqlQueryLoader
    {
        private static readonly ConcurrentDictionary<string, string> _queryCache = new();
        private static readonly string _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        /// <summary>
        /// Loads a SQL query from a file with caching
        /// </summary>
        /// <param name="category">The category folder (e.g., "Books", "Users", "Lendings", "Reservations")</param>
        /// <param name="queryName">The name of the SQL file without extension (e.g., "GetBookById")</param>
        /// <returns>The SQL query string</returns>
        public static string LoadQuery(string category, string queryName)
        {
            var cacheKey = $"{category}.{queryName}";
            
            if (_queryCache.TryGetValue(cacheKey, out var cachedQuery))
            {
                return cachedQuery;
            }

            var filePath = Path.Combine(_basePath, category, $"{queryName}.sql");
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"SQL query file not found: {filePath}");
            }

            var query = File.ReadAllText(filePath);
            _queryCache[cacheKey] = query;
            
            return query;
        }

        /// <summary>
        /// Clears the query cache (useful for development/testing)
        /// </summary>
        public static void ClearCache()
        {
            _queryCache.Clear();
        }
    }
}
