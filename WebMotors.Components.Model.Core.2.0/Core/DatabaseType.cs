using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace WebMotors.Components.Model.Core
{
	public class DatabaseType<T> : Database, IDisposable
		where T : DbProviderFactory
	{
		public DatabaseType(string connection, bool automaticOpenConnection = true, IConfigurationRoot configuration = null, Action<string> log = null)
			: base(typeof(T), connection, automaticOpenConnection, configuration, log)
		{
		}
	}
}
