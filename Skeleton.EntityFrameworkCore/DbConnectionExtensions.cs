using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace Skeleton.EntityFrameworkCore
{
    public static class DbConnectionExtensions
    {
		/// <summary>
		/// Even though this is an IDisposable, DO NOT DISPOSE IT, otherwise EF will get angry since it is a shared connection
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
	    public static DbConnection GetConnection(this DbContext context)
	    {
		    DbConnection connection = context.Database.GetDbConnection();
		    if (connection.State != ConnectionState.Open)
		    {
			    connection.Open();
		    }

		    return connection;
	    }
    }
}