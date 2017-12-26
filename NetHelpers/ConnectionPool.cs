using System;
using System.Collections;
using System.Data.SqlClient;
using System.Reflection;

namespace NetHelpers
{
    public class ConnectionPool
    {
        public string PoolIdentifier { get; internal set; }
        public int NumberOfConnections { get; internal set; }
        public static ConnectionPool GetConnectionPool(SqlConnection sqlConnection)
        {
            ConnectionPool connectionPool = new ConnectionPool();
            connectionPool.PoolIdentifier = sqlConnection.ConnectionString;

            Type sqlConnectionType = typeof(SqlConnection);
            FieldInfo _poolGroupFieldInfo =
              sqlConnectionType.GetField("_poolGroup", BindingFlags.NonPublic | BindingFlags.Instance);
            var dbConnectionPoolGroup =
              _poolGroupFieldInfo.GetValue(sqlConnection);

            if (dbConnectionPoolGroup != null)
            {

                FieldInfo _poolCollectionFieldInfo =
                  dbConnectionPoolGroup.GetType().GetField("_poolCollection",
                     BindingFlags.NonPublic | BindingFlags.Instance);

                var poolCollection = _poolCollectionFieldInfo.GetValue(dbConnectionPoolGroup) as IEnumerable;
                foreach (var poolEntry in poolCollection)
                {
                    var t = poolEntry.GetType();
                    var foundPool = t.GetProperty("Value").GetValue(poolEntry, null);
                    FieldInfo _objectListFieldInfo =
                       foundPool.GetType().GetField("_objectList",
                          BindingFlags.NonPublic | BindingFlags.Instance);
                    var listTDbConnectionInternal =
                       _objectListFieldInfo.GetValue(foundPool);
                    MethodInfo get_CountMethodInfo =
                        listTDbConnectionInternal.GetType().GetMethod("get_Count");
                    var numberOfConnections = get_CountMethodInfo.Invoke(listTDbConnectionInternal, null);
                    connectionPool.NumberOfConnections = (Int32)numberOfConnections;
                }
            }
            return connectionPool;
        }
    }
}
