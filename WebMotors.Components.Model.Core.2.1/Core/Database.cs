using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;

namespace WebMotors.Components.Model.Core
{
	public class Database : IDisposable
	{
		#region [ +private fields ]

		private const string MySqlDataConnectionType = "mysqlconnection";
		private const string SqlServerDataConnectionType = "system.data.sqlclient.sqlconnection";
		private const string ElasticSearchDataConnectionType = "webmotors.elasticsearchconnector.connection";
		private string _typeFactory = string.Empty;
		private string _stringConnection = string.Empty;
		private DbProviderFactory _factory = null;
		private DbConnection _connection;
		private DbTransaction _transaction;
		private Action<string> _log = null;

		#endregion

		#region [ +Constructors ]

		public Database(string connection, bool automaticOpenConnection = true, IConfigurationRoot configuration = null, Action<string> log = null)
		{
			_log = log;

			if (configuration == null)
				configuration = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
					.AddJsonFile("appsettings.json", optional: true)
					.Build();

			_stringConnection = $"{configuration["connectionstrings:{0}:connectionstring".FormatStr(connection)]}";
			_typeFactory = $"{configuration["connectionstrings:{0}:providername".FormatStr(connection)]}";
			Log(_stringConnection);
			Log(_typeFactory);
			var assemblyName = _typeFactory.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
			var assemblyType = _typeFactory.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
			Log(assemblyName);
			Log(assemblyType);
			Type typeConnection = Assembly.Load(new AssemblyName(assemblyName)).GetType(assemblyType);
			Log(typeConnection.ToString());
			if (_typeFactory.Contains("System.Data.SqlClient.SqlClientFactory"))
				_factory = System.Data.SqlClient.SqlClientFactory.Instance;
			else
				_factory = (DbProviderFactory)Activator.CreateInstance(typeConnection);
			Log(_factory.ToString());
			CreateConnection(automaticOpenConnection);
		}

		public Database(Type typeConnection, string connection, bool automaticOpenConnection = true, IConfigurationRoot configuration = null, Action<string> log = null)
		{
			_log = log;
			if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")))
			{
				if (configuration == null)
					configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
				_stringConnection = $"{configuration["connectionstrings:{0}:connectionstring".FormatStr(connection)]}";
			}
			if (string.IsNullOrWhiteSpace(_stringConnection))
				_stringConnection = connection;
			_typeFactory = typeConnection.ToString();
			Log(typeConnection.ToString());
			if (_typeFactory.Contains("System.Data.SqlClient.SqlClientFactory"))
				_factory = System.Data.SqlClient.SqlClientFactory.Instance;
			else
				_factory = (DbProviderFactory)Activator.CreateInstance(typeConnection);
			Log(_factory.ToString());
			CreateConnection(automaticOpenConnection);
		}

		public Database(DbProviderFactory factory, string connection, bool automaticOpenConnection = true, IConfigurationRoot configuration = null, Action<string> log = null)
		{
			_log = log;
			if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")))
			{
				if (configuration == null)
					configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
				_stringConnection = $"{configuration["connectionstrings:{0}:connectionstring".FormatStr(connection)]}";
			}
			if (string.IsNullOrWhiteSpace(_stringConnection))
				_stringConnection = connection;
			_factory = factory;
			Log(_factory.ToString());
			CreateConnection(automaticOpenConnection);
		}

		#endregion

		#region [ +Properties ]

		#region [ StringConection ]
		public string StringConection
		{
			get { return _stringConnection; }
			set { _stringConnection = value; }
		}
		#endregion

		#region [ Connection ]
		public DbConnection Connection
		{
			get
			{
				return _connection;
			}
			set { _connection = value; }
		}
		#endregion

		#region [ Transaction ]
		public DbTransaction Transaction
		{
			get { return _transaction; }
			set { _transaction = value; }
		}
		#endregion

		#endregion

		#region [ +Methods ]

		#region [ isMysql ]
		public bool isElasticSearch
		{
			get { return _connection.GetType().ToString().ToLower().Contains(ElasticSearchDataConnectionType); }
		}
		#endregion

		#region [ isMysql ]
		public bool isMysql
		{
			get { return _connection.GetType().ToString().ToLower().Contains(MySqlDataConnectionType); }
		}
		#endregion

		#region [ isSqlServer ]
		public bool isSqlServer
		{
			get { return _connection.GetType().ToString().ToLower().Contains(SqlServerDataConnectionType); }
		} 
		#endregion

		#region [ getNOLOCKString ]
		internal string getNOLOCKString
		{
			get { return this.isSqlServer ? " (NOLOCK)" : string.Empty; }
		} 
		#endregion

		#region [ CreateConnection ]
		public void CreateConnection(bool openConnection = true)
		{
			Log("CreateConnection Start");
			if (_connection == null)
			{
				_connection = _factory.CreateConnection();
				_connection.ConnectionString = _stringConnection;
				if (openConnection)
					_connection.Open();
			}
			Log("CreateConnection End");
		}
		#endregion

		#region [ CreateTransaction ]
		public void CreateTransaction()
		{
			_transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
		}
		#endregion

		#region [ CreateCommand ]
		public DbCommand CreateCommand()
		{
			return CreateCommand(string.Empty);
		}
		#endregion

		#region [ CreateCommand ]

		public DbCommand CreateCommand(string sql)
		{
			DbCommand cmd = _factory.CreateCommand();

			if (_transaction != null)
			{
				cmd.Connection = _transaction.Connection;
				cmd.Transaction = _transaction;
			}
			else
				cmd.Connection = _connection;

			cmd.CommandText = sql;

			return cmd;
		}

		#endregion

		#region [ CreateParameter ]

		public DbParameter CreateParameter()
		{
			return CreateParameter("", null, DbType.Object, ParameterDirection.Input, int.MinValue);
		}

		#endregion

		#region [ CreateParameter ]

		public DbParameter CreateParameter(string nomeParametro, object valor)
		{
			return CreateParameter(nomeParametro, valor, DbType.Object, ParameterDirection.Input, int.MinValue);
		}

		#endregion

		#region [ CreateParameter ]

		public DbParameter CreateParameter(string nomeParametro, object valor, DbType tipoDado)
		{
			return CreateParameter(nomeParametro, valor, tipoDado, ParameterDirection.Input, int.MinValue);
		}

		#endregion

		#region [ CreateParameter ]

		public DbParameter CreateParameter(string nomeParametro, object valor, DbType tipoDado, ParameterDirection direcao)
		{
			return CreateParameter(nomeParametro, valor, tipoDado, direcao, int.MinValue);
		}

		#endregion

		#region [ CreateParameter ]

		public DbParameter CreateParameter(string nomeParametro, object valor, DbType tipoDado, ParameterDirection direcao, int tamanho)
		{
			DbParameter prm = _factory.CreateParameter();
			if (prm is SqlParameter)
				ConfigureSqlParameter((SqlParameter)prm, valor);
			prm.ParameterName = nomeParametro;
			prm.Value = valor;
			if (tipoDado != DbType.Object)
				prm.DbType = tipoDado;
			prm.Direction = direcao;
			if (tamanho > 0)
				prm.Size = tamanho;
			return prm;
		}

		private void ConfigureSqlParameter(SqlParameter prm, object valor)
		{
			if (valor is String)
				prm.SqlDbType = SqlDbType.VarChar;
		}

		#endregion

		#region [ CloseConnection ]
		public void CloseConnection()
		{
			if (_connection != null && _connection.State != ConnectionState.Closed)
			{
				MySqlClearAllPoolsAsync();
				_connection.Close();
				_connection.Dispose();
			}
			_connection = null;
		}

		private void MySqlClearAllPoolsAsync()
		{
			if (this.isMysql)
			{
				var methods = _connection.GetType().GetMethods();
				foreach(var methodInfo in methods)
					if (methodInfo.Name == "ClearAllPoolsAsync")
					{
						object result = null;
						ParameterInfo[] parameters = methodInfo.GetParameters();
						result = methodInfo.Invoke(_connection, null);
						break;
					}
			}
		}
		#endregion

		#region [ ScopeIdentity ]
		public string ScopeIdentity
		{
			get
			{
				if (this.isMysql)
					return "SELECT LAST_INSERT_ID()";
				return "SELECT SCOPE_IDENTITY()";
			}
		}
		#endregion

		#region [ Paging ]
		public string SqlStringPaging(string sql, int page, int pageSize, string orderBy = null)
		{
			if (this.isMysql || this.isElasticSearch)
				return MySqlSqlStringPaging(sql, page, pageSize);
			return SqlServerSqlStringPaging(sql, page, pageSize, orderBy);
		}

		private string SqlServerSqlStringPaging(string sql, int page, int pageSize, string orderBy)
		{
			int start = 0;
			int end = pageSize;

			if (page > 1)
			{
				end = (pageSize * page);
				start = end - pageSize;
			}

			StringBuilder sbSqlPaginado = new StringBuilder();
			sbSqlPaginado.Append("WITH FindPaging AS ( ");
			sbSqlPaginado.Append(sql);
			sbSqlPaginado.Append(") ");
			sbSqlPaginado.AppendFormat("SELECT TOP {0} * ", pageSize);
			sbSqlPaginado.AppendFormat("FROM (SELECT ROW_NUMBER() OVER (ORDER BY {0}) AS Line ", orderBy);
			sbSqlPaginado.Append(",(SELECT COUNT(*) FROM FindPaging) AS TotalRecords ");
			sbSqlPaginado.Append(", * FROM FindPaging) AS FindPaging ");
			sbSqlPaginado.AppendFormat("WHERE Line > {0} AND Line <= {1} ", start, end);

			return sbSqlPaginado.ToString();
		}

		private string MySqlSqlStringPaging(string sql, int page, int pageSize)
		{
			int start = 0;
			if (page > 1)
				start = ((page - 1) * pageSize);
			return string.Format("{0} limit {1}, {2}; SELECT FOUND_ROWS();", sql, start, pageSize);
		}

		internal int InWhileDR(DbDataReader dr)
		{
			if (this.isSqlServer)
				return dr.GetInt32(dr.GetOrdinal("TotalRecords"));
			return 0;
		}

		internal int AfterWhileDR(DbDataReader dr, int totalRecords)
		{
			if (this.isMysql || this.isElasticSearch)
			{
				dr.NextResult();
				while (dr.Read())
				{
					return dr.GetInt32(0);
				}
			}
			return totalRecords;
		}
		#endregion

		#region [ Dispose ]
		public void Dispose()
		{
			this.Rollback();
			this.CloseConnection();
		}
		#endregion

		#region [ Commit ]
		public void Commit()
		{
			if (_transaction != null)
			{
				_transaction.Commit();
				_transaction.Dispose();
				_transaction = null;
			}
		}
		#endregion

		#region [ Rollback ]
		public void Rollback()
		{
			if (_transaction != null)
			{
				_transaction.Rollback();
				_transaction.Dispose();
				_transaction = null;
			}
		}
		#endregion

		#region [ Log ]
		private void Log(string message)
		{
			if (_log != null)
				_log(message);
		}
		#endregion

		#endregion
	}
}
