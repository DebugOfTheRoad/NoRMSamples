using System.Configuration;
using System.Linq;
using Norm.Linq;

namespace NoRMSamples
{
    internal static class TestHelper
    {
        private static readonly string _connectionStringHost = ConfigurationManager.AppSettings["connectionStringHost"];

        public static string ConnectionString()
        {
            return ConnectionString(null);
        }

        public static string ConnectionString(string query)
        {
            return ConnectionString(query, null, null, null);
        }

        public static string ConnectionString(string userName, string password)
        {
            return ConnectionString(null, null, userName, password);
        }

        public static string ConnectionString(string query, string userName, string password)
        {
            return ConnectionString(query, null, userName, password);
        }

        public static string ConnectionString(string query, string database, string userName, string password)
        {
            var authentication = string.Empty;
            if (userName != null)
            {
                authentication = string.Concat(userName, ':', password, '@');
            }
            if (!string.IsNullOrEmpty(query) && !query.StartsWith("?"))
            {
                query = string.Concat('?', query);
            }
            var host = (string.IsNullOrEmpty(_connectionStringHost) ? "localhost" : _connectionStringHost) + ":" + ConfigurationManager.AppSettings["testPort"];
            database = database ?? "NormTests";
            return string.Format("mongodb://{0}{1}/{2}{3}", authentication, host, database, query);
        }
        public static QueryTranslationResults QueryStructure<T>(this IQueryable<T> queryable)
        {
            return ((IMongoQueryResults)queryable.Provider).TranslationResults;
        }
    }
}