using System;
using System.Data;
using System.Data.Common;

namespace WebMotors.Components.Model.Interfaces
{
	public interface IDatabase : IDisposable
	{
		DbConnection Connection { get; set; }
		bool isElasticSearch { get; }
		bool isMysql { get; }
		bool isSqlServer { get; }
		string ScopeIdentity { get; }
		string StringConection { get; set; }
		DbTransaction Transaction { get; set; }
		string TypeConnection { get; set; }
		void CloseConnection();
		void Commit();
		DbCommand CreateCommand();
		DbCommand CreateCommand(string sql);
		void CreateConnection(bool openConnection = true);
		DbDataAdapter CreateDataAdapter();
		DbParameter CreateParameter();
		DbParameter CreateParameter(string nomeParametro, object valor);
		DbParameter CreateParameter(string nomeParametro, object valor, DbType tipoDado);
		DbParameter CreateParameter(string nomeParametro, object valor, DbType tipoDado, ParameterDirection direcao);
		DbParameter CreateParameter(string nomeParametro, object valor, DbType tipoDado, ParameterDirection direcao, int tamanho);
		void CreateTransaction();
		void Rollback();
		string SqlStringPaging(string sql, int page, int pageSize, string orderBy = null);
	}
}
